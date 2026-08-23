using Auth.Data;
using Auth.Model.DTOs.Teachers;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Auth.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TeacherService> _logger;

        public TeacherService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<TeacherService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> RegisterTeacher(CreateTeacherDto teacherDto)
        {
            if (teacherDto == null)
                return new BadRequestObjectResult("Invalid teacher data.");

            // Check duplicate email
            var existingUser = await _userManager.FindByEmailAsync(teacherDto.Email);
            if (existingUser != null)
                return new BadRequestObjectResult("A user with this email already exists.");

            // Check duplicate CNIC
            var cnicExists = await _context.Teachers.AnyAsync(t => t.CNIC == teacherDto.CNIC);
            if (cnicExists)
                return new BadRequestObjectResult("A teacher with this CNIC already exists.");

            var user = new ApplicationUser
            {
                UserName = teacherDto.Email,
                Email = teacherDto.Email,
                FullName = teacherDto.FullName
            };

            // Generate a random temp password instead of using CNIC (CNIC is not a secret)
            var tempPassword = GenerateTemporaryPassword();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var createResult = await _userManager.CreateAsync(user, tempPassword);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new BadRequestObjectResult(createResult.Errors.Select(e => e.Description));
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "Teacher");
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    await _userManager.DeleteAsync(user);
                    return new BadRequestObjectResult(roleResult.Errors.Select(e => e.Description));
                }

                var teacher = new Teacher
                {
                    UserId = user.Id,
                    CNIC = teacherDto.CNIC,
                    Qualification = teacherDto.Qualification,
                    IdentificationNumber = teacherDto.IdentificationNumber,
                    Department = teacherDto.Department,
                    DateOfBirth = teacherDto.DateOfBirth,
                    HireDate = teacherDto.HireDate,
                    Address = teacherDto.Address,
                    Salary = teacherDto.Salary,
                    IsActive = teacherDto.IsActive
                };

                await _context.Teachers.AddAsync(teacher);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new OkObjectResult(new
                {
                    Message = "Teacher registered successfully.",
                    UserId = user.Id,
                    UserEmail = user.Email 
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _userManager.DeleteAsync(user); // cleanup outside the DB transaction (Identity store)
                _logger.LogError(ex, "Failed to register teacher with email {Email}", teacherDto.Email);

                return new BadRequestObjectResult("Teacher registration failed. Please try again.");
            }
        }

        public async Task<IActionResult> UpdateTeacherDetails(TeacherDetailDto teacherDto)
        {
            
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.Email == teacherDto.Email);

            if (teacher == null)
                return new BadRequestObjectResult("Teacher doesn't exist.");

      
            teacher.Address = teacherDto.Address;
            teacher.IsActive = teacherDto.IsActive;
            teacher.CNIC = teacherDto.CNIC;
            teacher.DateOfBirth = teacherDto.DateOfBirth;
            teacher.Department = teacherDto.Department;
            teacher.Salary = teacherDto.Salary;
            teacher.HireDate = teacherDto.HireDate;
            teacher.IdentificationNumber = teacherDto.IdentificationNumber;
            teacher.Qualification = teacherDto.Qualification;
            teacher.User.FullName = teacherDto.Fullname;
            

            try
            {
                await _context.SaveChangesAsync();
                return new OkObjectResult(new
                {
                    Message = "Teacher details updated successfully.",
                    TeacherId = teacher.User.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update teacher with email {Email}", teacherDto.Email);
                return new BadRequestObjectResult("Failed to update teacher details. Please try again.");
            }
        }

        public async Task<List<TeacherDetailDto>> GetAllTechers()
        {
            return await _context.Teachers.Include(t => t.User).Select(t => new TeacherDetailDto
            {
                Teacher_id=t.Teacher_Id,
                Address = t.Address,
                Fullname = t.User.FullName,
                IsActive = t.IsActive,
                CNIC = t.CNIC,
                DateOfBirth = t.DateOfBirth,
                Department = t.Department,
                Salary = t.Salary,
                Email = t.User.Email,
                HireDate = t.HireDate,
                IdentificationNumber = t.IdentificationNumber,
                Qualification = t.Qualification
            }).ToListAsync();
        }

        public async Task<IActionResult> DeleteTeacher(Guid teacherId)
        {
            // Find teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Teacher_Id == teacherId);

            if (teacher == null)
            {
                return new BadRequestObjectResult("Teacher Not Found");
            }

            // Find associated Identity user
            var user = await _userManager.FindByIdAsync(teacher.UserId);

            if (user == null)
            {
                return new BadRequestObjectResult("Associated user not found");
            }

            // Mark teacher as inactive
            teacher.IsActive = false;

            // Disable Identity login
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            // remove teacher role
            await _userManager.RemoveFromRoleAsync(user, "Teacher");

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Teacher has been deactivated successfully."
            });
        }

        public async Task<Teacher> GetTeacherByID(Guid teacher_id)
        {
            return await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Teacher_Id == teacher_id);
         

       
        }

        // restore teacher
        public async Task<bool> RestoreTeacher(Guid teacherId)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Teacher_Id == teacherId);

            if (teacher == null)
            {
                return false;
            }

            teacher.IsActive = true;
            teacher.User.LockoutEnabled = true;
            teacher.User.LockoutEnd = null;

            if (!await _userManager.IsInRoleAsync(teacher.User, "Teacher"))
            {
                var result = await _userManager.AddToRoleAsync(
                    teacher.User,
                    "Teacher"
                );

                if (!result.Succeeded)
                {
                    return false;
                }
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IActionResult> PromoteToHOD(Guid teacherId)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Teacher_Id == teacherId);
            if (teacher == null)
            {
                return new BadRequestObjectResult("Teacher not found.");
            }
            // Check if the user is already an HOD
            if (await _userManager.IsInRoleAsync(teacher.User, "HOD"))
            {
                return new BadRequestObjectResult("Teacher is already an HOD.");
            }
            // Remove Teacher role and add HOD role
            var removeResult = await _userManager.RemoveFromRoleAsync(teacher.User, "Teacher");
            if (!removeResult.Succeeded)
            {
                return new BadRequestObjectResult("Failed to remove Teacher role.");
            }
            var addResult = await _userManager.AddToRoleAsync(teacher.User, "HOD");
            if (!addResult.Succeeded)
            {
                // Rollback: Add Teacher role back
                await _userManager.AddToRoleAsync(teacher.User, "Teacher");
                return new BadRequestObjectResult("Failed to promote to HOD.");
            }
            return new OkObjectResult(new
            {
                message = "Teacher promoted to HOD successfully."
            });
        }
        public async Task<IActionResult> DemoteToTeacher(Guid teacherId)
        {
            var teacher = await _context.Teachers.Include(t => t.User).FirstOrDefaultAsync(t => t.Teacher_Id == teacherId);
            // check if already a teacher
            if(await _userManager.IsInRoleAsync(teacher.User,"Teacher"))
            {
                return new BadRequestObjectResult("Already a teacher");
            }
            // remove HOD role:
            var issucceded = await _userManager.RemoveFromRoleAsync(teacher.User, "HOD");
            if(!issucceded.Succeeded)
            {
                return new BadRequestObjectResult("unable to change");
            }
            else
            {
                await _userManager.AddToRoleAsync(teacher.User, "Teacher");
                return new OkObjectResult("Role has been updated to Teacher");
            }

            

             

        }
        private static string GenerateTemporaryPassword()
        {
            var randomPart = Convert.ToBase64String(RandomNumberGenerator.GetBytes(6))
                .Replace("+", "").Replace("/", "").Replace("=", "");
            return "Password@123";
        }


        // export teacher
        public async Task<IActionResult> ExportTeachers()
        {
            var teacherDetails = await _context
                .Teachers
                .Include(t => t.User)
                .Include(t => t.TeacherAssignments)
                    .ThenInclude(ta => ta.Section)
                .Include(t => t.TeacherAssignments)
                    .ThenInclude(ta => ta.Course)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Teacherdetails");

            // Headers
            worksheet.Cell(1, 1).Value = "Identification Number";
            worksheet.Cell(1, 2).Value = "FullName";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Department";
            worksheet.Cell(1, 5).Value = "CNIC";
            worksheet.Cell(1, 6).Value = "Address";
            worksheet.Cell(1, 7).Value = "Courses";
            worksheet.Cell(1, 8).Value = "Sections";


            // Data - one row per teacher
            for (int i = 0; i < teacherDetails.Count; i++)
            {
                var teacher = teacherDetails[i];
                var row = i + 2;

                var sections = teacher.TeacherAssignments != null
                    ? string.Join(", ", teacher.TeacherAssignments
                        .Where(a => a.Section != null)
                        .Select(a => a.Section.SectionName))
                    : string.Empty;

                var courses = teacher.TeacherAssignments != null
                    ? string.Join(", ", teacher.TeacherAssignments
                        .Where(a => a.Course != null)
                        .Select(a => a.Course.CourseName))
                    : string.Empty;

                worksheet.Cell(row, 1).Value = teacher.IdentificationNumber;
                worksheet.Cell(row, 2).Value = teacher.User.FullName;
                worksheet.Cell(row, 3).Value = teacher.User.Email;
                worksheet.Cell(row, 4).Value = teacher.Department;
                worksheet.Cell(row, 5).Value = teacher.CNIC;
                worksheet.Cell(row, 6).Value = teacher.Address;
                worksheet.Cell(row, 7).Value = sections;
                worksheet.Cell(row, 8).Value = courses;
            }
            // Format headers
            worksheet.Row(1).Style.Font.Bold = true;

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            var content = stream.ToArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "TeacherList.xlsx";

            return new Microsoft.AspNetCore.Mvc.FileContentResult(content, contentType)
            {
                FileDownloadName = fileName
            };
        }


    }
}
