using Auth.Data;
using Auth.Model.DTOs.Student;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
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

        public async Task<IActionResult> CreateStudent(StudentCreateDto studentCreateDto)
        {
            if (studentCreateDto == null || string.IsNullOrWhiteSpace(studentCreateDto.Email))
                return new BadRequestObjectResult("Email is required.");

            var exists = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(f => f.User != null && f.User.Email == studentCreateDto.Email);

            if (exists != null)
                return new ConflictObjectResult("Student with this email already exists.");

            // Validate the section exists BEFORE creating the Identity user,
            // so we don't create an orphaned user if this fails.
            var sectionExists = await _context.Sections.AnyAsync(s => s.SectionId == studentCreateDto.SectionId);
            if (!sectionExists)
                return new BadRequestObjectResult("Section does not exist.");

            var applicationUser = new ApplicationUser
            {
                UserName = studentCreateDto.Email,
                Email = studentCreateDto.Email,
                PhoneNumber = studentCreateDto.PhoneNumber,
                FullName = studentCreateDto.FullName
            };

            // Create the Identity user FIRST — must succeed before it can be assigned a role
            var result = await _userManager.CreateAsync(applicationUser, "DefaultPassword123!");
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return new BadRequestObjectResult(errors);
            }

            // Now assign the role, since applicationUser has a valid Id
            var roleResult = await _userManager.AddToRoleAsync(applicationUser, "Student");
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description).ToList();
                // Roll back the created user so we don't leave an orphaned account
                await _userManager.DeleteAsync(applicationUser);
                return new BadRequestObjectResult(errors);
            }

            var student = new Student
            {
                UserId = applicationUser.Id,
                DateOfBirth = studentCreateDto.DateOfBirth,
                EnrollmentDate = studentCreateDto.EnrollmentDate,
                CNIC = studentCreateDto.CNIC
            };

            // Add student FIRST — this is what triggers EF Core's client-side
            // Guid generator to populate student.StudentId. Reading it before
            // this call gives you Guid.Empty, which is what caused the FK error.
            _context.Students.Add(student);

            var enrollment = new StudentEnrollments
            {
                StudentId = student.StudentId,   // now correctly populated
                SectionId = studentCreateDto.sectionId,
                IsActive = true,
                EnrolledDate = DateTime.UtcNow
            };
            _context.StudentEnrollments.Add(enrollment);

            await _context.SaveChangesAsync();

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
                    CNIC = s.CNIC
                })
                .ToListAsync();
        }

        public async Task<StudentDto?> GetStudentById(Guid studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null) return null;

            return new StudentDto
            {
                StudentId = student.StudentId,
                UserId = student.UserId,
                FullName = student.User.FullName,
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                EnrollmentDate = student.EnrollmentDate,
                CNIC = student.CNIC
            };
        }

        public async Task<IActionResult> UpdateStudent(Guid studentId, StudentUpdateDto dto)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return new NotFoundObjectResult("Student not found.");

            // Update Student fields
            student.DateOfBirth = dto.DateOfBirth;
            student.EnrollmentDate = dto.EnrollmentDate;
            student.CNIC = dto.CNIC;
            student.User.FullName = dto.FullName;
            student.User.Email = dto.Email;
            student.User.PhoneNumber = dto.PhoneNumber;



            await _context.SaveChangesAsync();
            return new OkResult();
        }

        public async Task<IActionResult> DeleteStudent(Guid studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return new NotFoundResult();

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            // Also remove the linked Identity user
            if (student.User != null)
                await _userManager.DeleteAsync(student.User);

            return new OkResult();
        }
    }
}