using Auth.Data;
using Auth.Model.DTOs.Student;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

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
                        PhoneNumber = studentCreateDto.PhoneNumber,
                        FullName = studentCreateDto.FullName
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
                        DateOfBirth = studentCreateDto.DateOfBirth,
                        EnrollmentDate = studentCreateDto.EnrollmentDate,
                        CNIC = studentCreateDto.CNIC
                    };

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

                    var studentDto = new StudentDto
                    {
                        StudentId = student.StudentId,
                        UserId = applicationUser.Id,
                        FullName = applicationUser.FullName,
                        Email = applicationUser.Email,
                        PhoneNumber = applicationUser.PhoneNumber,
                        DateOfBirth = student.DateOfBirth,
                        EnrollmentDate = student.EnrollmentDate,
                        CNIC = student.CNIC,
                    };

                    return new OkObjectResult(studentDto);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static string FormatSectionLabel(Section? section)
        {
            if (section == null) return string.Empty;
            if (string.IsNullOrWhiteSpace(section.IntermediateClass))
                return section.SectionName;
            return $"{section.IntermediateClass} - {section.SectionName}";
        }

        public async Task<List<StudentDto>> GetAllStudents()
        {
            return await _context.Students
                .Include(s => s.User)
                .Select(s => new StudentDto
                {
                    StudentId = s.StudentId,
                    UserId = s.UserId,
                    FullName = s.User.FullName,
                    Email = s.User.Email,
                    PhoneNumber = s.User.PhoneNumber,
                    DateOfBirth = s.DateOfBirth,
                    EnrollmentDate = s.EnrollmentDate,
                    CNIC = s.CNIC,
                    SectionId = s.StudentEnrollments
                        .Where(e => e.IsActive)
                        .Select(e => (Guid?)e.SectionId)
                        .FirstOrDefault(),
                    Section = s.StudentEnrollments
                        .Where(e => e.IsActive && e.EnrolledSection != null)
                        .Select(e => e.EnrolledSection!.IntermediateClass + " - " + e.EnrolledSection.SectionName)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();
        }

        public async Task<StudentDto?> GetStudentById(string studentId)
        {
            Student? student = null;
            if (Guid.TryParse(studentId, out var guid))
            {
                student = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.StudentEnrollments)
                        .ThenInclude(se => se.EnrolledSection)
                    .FirstOrDefaultAsync(s => s.StudentId == guid || s.UserId == studentId);
            }
            else
            {
                student = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.StudentEnrollments)
                        .ThenInclude(se => se.EnrolledSection)
                    .FirstOrDefaultAsync(s => s.UserId == studentId);
            }

            if (student == null) return null;

            var enrollment = student.StudentEnrollments.FirstOrDefault(e => e.IsActive)
                ?? student.StudentEnrollments.FirstOrDefault();

            return new StudentDto
            {
                StudentId = student.StudentId,
                UserId = student.UserId,
                FullName = student.User.FullName,
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                EnrollmentDate = student.EnrollmentDate,
                CNIC = student.CNIC,
                SectionId = enrollment?.SectionId,
                Section = FormatSectionLabel(enrollment?.EnrolledSection)
            };
        }

        public async Task<IActionResult> UpdateStudent(Guid studentId, StudentUpdateDto dto)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.StudentEnrollments)
                    .ThenInclude(se => se.EnrolledSection)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return new NotFoundObjectResult("Student not found.");

            student.DateOfBirth = dto.DateOfBirth;
            student.EnrollmentDate = dto.EnrollmentDate;
            student.CNIC = dto.CNIC;

            if (student.User != null)
            {
                student.User.FullName = dto.FullName;
                student.User.PhoneNumber = dto.PhoneNumber;
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

            await _context.SaveChangesAsync();

            await _context.Entry(student).Collection(s => s.StudentEnrollments).Query()
                .Include(se => se.EnrolledSection)
                .LoadAsync();

            var activeEnrollment = student.StudentEnrollments.FirstOrDefault(e => e.IsActive)
                ?? student.StudentEnrollments.FirstOrDefault();

            var updated = new StudentDto
            {
                StudentId = student.StudentId,
                UserId = student.UserId,
                FullName = student.User?.FullName,
                Email = student.User?.Email,
                PhoneNumber = student.User?.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                EnrollmentDate = student.EnrollmentDate,
                CNIC = student.CNIC,
                SectionId = activeEnrollment?.SectionId,
                Section = FormatSectionLabel(activeEnrollment?.EnrolledSection)
            };

            return new OkObjectResult(updated);
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
                    FullName = s.User.FullName,
                    CNIC=s.CNIC,
                    DateOfBirth=s.DateOfBirth,
                    Email=s.User.Email,
                    PhoneNumber=s.User.PhoneNumber,
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
                .Include(s => s.User)
                .Select(s => new
                {
                    s.StudentId,
                    FullName = s.User.FullName,
                    Email = s.User.Email,
                    PhoneNumber = s.User.PhoneNumber,
                    s.DateOfBirth,
                    s.EnrollmentDate,
                    s.CNIC
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Students");

            // Headers
            worksheet.Cell(1, 1).Value = "StudentId";
            worksheet.Cell(1, 2).Value = "FullName";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "PhoneNumber";
            worksheet.Cell(1, 5).Value = "DateOfBirth";
            worksheet.Cell(1, 6).Value = "EnrollmentDate";
            worksheet.Cell(1, 7).Value = "CNIC";

            // Data
            for (int i = 0; i < students.Count; i++)
            {
                var student = students[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = student.StudentId.ToString();
                worksheet.Cell(row, 2).Value = student.FullName;
                worksheet.Cell(row, 3).Value = student.Email;
                worksheet.Cell(row, 4).Value = student.PhoneNumber;
                worksheet.Cell(row, 5).Value = student.DateOfBirth;
                worksheet.Cell(row, 6).Value = student.EnrollmentDate;
                worksheet.Cell(row, 7).Value = student.CNIC;
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