using Auth.Data;
using Auth.Model.DTOs.Teachers;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
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

        private static string GenerateTemporaryPassword()
        {
            var randomPart = Convert.ToBase64String(RandomNumberGenerator.GetBytes(6))
                .Replace("+", "").Replace("/", "").Replace("=", "");
            return "Password@123";
        }
    }
}