using System.Linq.Expressions;
using Auth.Data;
using Auth.Model.DTOs.Student;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentService(ApplicationDbContext applicationDbContext, UserManager<ApplicationUser> userManager)
        {
            _context = applicationDbContext;
            _userManager = userManager;
        }

        /// <summary>Single projection used by every read so all endpoints return the same student shape.</summary>
        private static readonly Expression<Func<Student, StudentDto>> ToDto = s => new StudentDto
        {
            StudentId = s.StudentId,
            UserId = s.UserId,
            StudentCode = s.StudentCode,
            FullName = s.User.FullName,
            Email = s.User.Email,
            PhoneNumber = s.User.PhoneNumber,
            DateOfBirth = s.DateOfBirth,
            EnrollmentDate = s.EnrollmentDate,
            CNIC = s.CNIC,
            DepartmentId = s.DepartmentId,
            Department = s.Department != null ? s.Department.Name : string.Empty,
            CohortId = s.CohortId,
            Cohort = s.Cohort != null ? s.Cohort.Name : string.Empty,
            CampusId = s.CampusId,
            Campus = s.Campus != null ? s.Campus.Name : string.Empty,
            MajorSubject = s.MajorSubject,
            Gender = s.Gender,
            FatherName = s.FatherName,
            IsActive = s.IsActive,
            SectionId = s.StudentEnrollments
                .Where(e => e.IsActive)
                .Select(e => (Guid?)e.SectionId)
                .FirstOrDefault(),
            Section = s.StudentEnrollments
                .Where(e => e.IsActive && e.EnrolledSection != null)
                .Select(e => e.EnrolledSection!.IntermediateClass + " - " + e.EnrolledSection.SectionName)
                .FirstOrDefault() ?? string.Empty
        };

        public async Task<IActionResult> CreateStudent(StudentCreateDto studentCreateDto)
        {
            if (studentCreateDto == null || string.IsNullOrWhiteSpace(studentCreateDto.Email))
                return new BadRequestObjectResult("Email is required.");

            var exists = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(f => f.User != null && f.User.Email == studentCreateDto.Email);

            if (exists != null)
                return new ConflictObjectResult("Student with this email already exists.");

            var sectionExists = await _context.Sections.AnyAsync(s => s.SectionId == studentCreateDto.SectionId);
            if (!sectionExists)
                return new BadRequestObjectResult("Section does not exist.");

            if (!await IsActiveDepartmentAsync(studentCreateDto.DepartmentId))
                return new BadRequestObjectResult("Choose a valid, active department.");

            var profileError = await ValidateProfileAsync(studentCreateDto, null);
            if (profileError != null)
                return profileError;

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var applicationUser = new ApplicationUser
                    {
                        UserName = studentCreateDto.Email,
                        Email = studentCreateDto.Email,
                        PhoneNumber = studentCreateDto.PhoneNumber.Trim(),
                        FullName = studentCreateDto.FullName.Trim()
                    };

                    var result = await _userManager.CreateAsync(applicationUser, "DefaultPassword123!");
                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        var errors = result.Errors.Select(e => e.Description).ToList();
                        return new BadRequestObjectResult(errors);
                    }

                    var roleResult = await _userManager.AddToRoleAsync(applicationUser, "Student");
                    if (!roleResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        var errors = roleResult.Errors.Select(e => e.Description).ToList();
                        return new BadRequestObjectResult(errors);
                    }

                    var student = new Student
                    {
                        UserId = applicationUser.Id,
                        EnrollmentDate = studentCreateDto.EnrollmentDate,
                        DepartmentId = studentCreateDto.DepartmentId,
                        IsActive = true
                    };
                    ApplyProfile(student, studentCreateDto);

                    _context.Students.Add(student);

                    var enrollment = new StudentEnrollments
                    {
                        StudentId = student.StudentId,
                        SectionId = studentCreateDto.SectionId,
                        IsActive = true,
                        EnrolledDate = DateTime.UtcNow
                    };
                    _context.StudentEnrollments.Add(enrollment);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new OkObjectResult(await GetDtoAsync(student.StudentId));
                }
                catch (DbUpdateException ex) when (IsUniqueCodeViolation(ex))
                {
                    await transaction.RollbackAsync();
                    return DuplicateCode(studentCreateDto.StudentCode);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<StudentDto>> GetAllStudents()
        {
            return await _context.Students
                .AsNoTracking()
                .OrderBy(s => s.StudentCode)
                .Select(ToDto)
                .ToListAsync();
        }

        public async Task<StudentDto?> GetStudentById(string studentId)
        {
            // Accepts a StudentId or the Identity user id (the mobile app signs in with the latter)
            var query = _context.Students.AsNoTracking();
            query = Guid.TryParse(studentId, out var guid)
                ? query.Where(s => s.StudentId == guid || s.UserId == studentId)
                : query.Where(s => s.UserId == studentId);

            return await query.Select(ToDto).FirstOrDefaultAsync();
        }

        public async Task<IActionResult> UpdateStudent(Guid studentId, StudentUpdateDto dto)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.StudentEnrollments)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return new NotFoundObjectResult("Student not found.");

            // Keeping the current department is allowed even if it was deactivated since
            if (dto.DepartmentId.HasValue && dto.DepartmentId != student.DepartmentId)
            {
                if (!await IsActiveDepartmentAsync(dto.DepartmentId.Value))
                    return new BadRequestObjectResult("Choose a valid, active department.");
                student.DepartmentId = dto.DepartmentId;
            }

            var profileError = await ValidateProfileAsync(dto, student);
            if (profileError != null)
                return profileError;

            ApplyProfile(student, dto);
            student.EnrollmentDate = dto.EnrollmentDate;

            if (student.User != null)
            {
                student.User.FullName = dto.FullName.Trim();
                student.User.PhoneNumber = dto.PhoneNumber.Trim();
                if (!string.IsNullOrWhiteSpace(dto.Email) && student.User.Email != dto.Email)
                {
                    student.User.Email = dto.Email;
                    student.User.UserName = dto.Email;
                }

                var identityResult = await _userManager.UpdateAsync(student.User);
                if (!identityResult.Succeeded)
                {
                    return new BadRequestObjectResult(identityResult.Errors.Select(e => e.Description));
                }
            }

            if (dto.SectionId.HasValue && dto.SectionId.Value != Guid.Empty)
            {
                var sectionExists = await _context.Sections.AnyAsync(s => s.SectionId == dto.SectionId.Value);
                if (!sectionExists)
                    return new BadRequestObjectResult("Section does not exist.");

                var enrollment = student.StudentEnrollments.FirstOrDefault(e => e.IsActive)
                    ?? student.StudentEnrollments.FirstOrDefault();

                if (enrollment != null)
                {
                    enrollment.SectionId = dto.SectionId.Value;
                    enrollment.IsActive = true;
                    enrollment.WithdrawnDate = null;
                }
                else
                {
                    _context.StudentEnrollments.Add(new StudentEnrollments
                    {
                        StudentEnrollmentId = Guid.NewGuid(),
                        StudentId = student.StudentId,
                        SectionId = dto.SectionId.Value,
                        EnrolledDate = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueCodeViolation(ex))
            {
                return DuplicateCode(dto.StudentCode);
            }

            return new OkObjectResult(await GetDtoAsync(student.StudentId));
        }

        /// <summary>Activates or deactivates a student. Deactivating also revokes saved sign-ins (mobile refresh tokens).</summary>
        public async Task<IActionResult> SetStudentStatus(Guid studentId, bool isActive)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
                return new NotFoundObjectResult("Student not found.");

            student.IsActive = isActive;
            if (!isActive)
            {
                await _context.RefreshTokens
                    .Where(t => t.UserId == student.UserId && !t.IsRevoked)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true));
            }
            await _context.SaveChangesAsync();

            return new OkObjectResult(await GetDtoAsync(studentId));
        }

        private Task<StudentDto?> GetDtoAsync(Guid studentId)
        {
            return _context.Students.AsNoTracking().Where(s => s.StudentId == studentId).Select(ToDto).FirstOrDefaultAsync();
        }

        /// <summary>Checks the code is free and the cohort/campus exist and are active (keeping a current, since-deactivated one is allowed).</summary>
        private async Task<IActionResult?> ValidateProfileAsync(StudentProfileFieldsDto dto, Student? existing)
        {
            var code = dto.StudentCode.Trim();
            var takenBy = existing?.StudentId;
            if (await _context.Students.AnyAsync(s => s.StudentCode == code && s.StudentId != takenBy))
                return DuplicateCode(code);

            if (dto.CohortId != existing?.CohortId && !await _context.Cohorts.AnyAsync(c => c.Id == dto.CohortId && c.IsActive))
                return new BadRequestObjectResult(new { message = "Choose a valid, active cohort." });

            if (dto.CampusId != existing?.CampusId && !await _context.Campuses.AnyAsync(c => c.Id == dto.CampusId && c.IsActive))
                return new BadRequestObjectResult(new { message = "Choose a valid, active campus." });

            return null;
        }

        private static void ApplyProfile(Student student, StudentProfileFieldsDto dto)
        {
            student.StudentCode = dto.StudentCode.Trim();
            student.CohortId = dto.CohortId;
            student.CampusId = dto.CampusId;
            student.MajorSubject = dto.MajorSubject.Trim();
            student.Gender = dto.Gender;
            student.FatherName = dto.FatherName.Trim();
            student.DateOfBirth = dto.DateOfBirth;
            student.CNIC = dto.CNIC;
        }

        private static IActionResult DuplicateCode(string code) =>
            new ConflictObjectResult(new { message = $"Student code {code.Trim()} is already assigned to another student." });

        // The unique index is the final guard if two requests race past the AnyAsync check
        private static bool IsUniqueCodeViolation(DbUpdateException ex) =>
            ex.InnerException?.Message.Contains("IX_Students_StudentCode") == true;

        private Task<bool> IsActiveDepartmentAsync(Guid departmentId)
        {
            return _context.Departments.AnyAsync(d => d.DepartmentId == departmentId && d.IsActive);
        }

        public async Task<IActionResult> DeleteStudent(Guid studentId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null)
                return new NotFoundObjectResult("Student not found.");

            var enrollments = _context.StudentEnrollments.Where(e => e.StudentId == studentId);
            _context.StudentEnrollments.RemoveRange(enrollments);

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new { message = "Deleted" });
        }

        public async Task<IActionResult> GetStudentBySectionId(Guid sectionId)
        {
            var students = await _context.Students
                .Where(s => s.StudentEnrollments.Any(e => e.SectionId == sectionId))
                .Select(s => new StudentMarksDto
                {
                    StudentId = s.StudentId,
                    EnrollmentId = s.StudentEnrollments.First(e => e.SectionId == sectionId).StudentEnrollmentId,
                    FullName = s.User.FullName

                })
                .ToListAsync();

            return new OkObjectResult(new { students });
        }

        public async Task<IActionResult> GetstudentDetailsBySectionID(Guid sectionId)
        {
            var students = await _context.Students
                .Where(s => s.StudentEnrollments.Any(e => e.SectionId == sectionId))
                .Select(s => new StudentDto
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    FullName = s.User.FullName,
                    CNIC=s.CNIC,
                    DateOfBirth=s.DateOfBirth,
                    Email=s.User.Email,
                    PhoneNumber=s.User.PhoneNumber,
                    IsActive = s.IsActive,
                   SectionId = sectionId,
                   Section = s.StudentEnrollments.First(e => e.SectionId == sectionId).EnrolledSection.IntermediateClass
                        + " - "
                        + s.StudentEnrollments.First(e => e.SectionId == sectionId).EnrolledSection.SectionName,

                })
                .ToListAsync();

            return new OkObjectResult(new { students });
        }
        // exports students to Excel
        public async Task<IActionResult> ExportStudentsToExcel()
        {
            var students = await _context.Students
                .AsNoTracking()
                .OrderBy(s => s.StudentCode)
                .Select(ToDto)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Students");

            // Headers
            string[] headers = { "Student Code", "Full Name", "Email", "Phone Number", "Father's Name", "Gender", "Major Subject", "Cohort", "Campus", "Section", "Status", "Date of Birth", "Enrollment Date", "CNIC" };
            for (int c = 0; c < headers.Length; c++)
            {
                worksheet.Cell(1, c + 1).Value = headers[c];
            }

            // Data
            for (int i = 0; i < students.Count; i++)
            {
                var student = students[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = student.StudentCode;
                worksheet.Cell(row, 2).Value = student.FullName;
                worksheet.Cell(row, 3).Value = student.Email;
                worksheet.Cell(row, 4).Value = student.PhoneNumber;
                worksheet.Cell(row, 5).Value = student.FatherName;
                worksheet.Cell(row, 6).Value = student.Gender;
                worksheet.Cell(row, 7).Value = student.MajorSubject;
                worksheet.Cell(row, 8).Value = student.Cohort;
                worksheet.Cell(row, 9).Value = student.Campus;
                worksheet.Cell(row, 10).Value = student.Section;
                worksheet.Cell(row, 11).Value = student.Status;
                worksheet.Cell(row, 12).Value = student.DateOfBirth;
                worksheet.Cell(row, 13).Value = student.EnrollmentDate;
                worksheet.Cell(row, 14).Value = student.CNIC;
            }

            // Format headers
            worksheet.Row(1).Style.Font.Bold = true;

            // Adjust column widths
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var content = stream.ToArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "Students.xlsx";

            return new Microsoft.AspNetCore.Mvc.FileContentResult(content, contentType)
            {
                FileDownloadName = fileName
            };
        }
    }
}
