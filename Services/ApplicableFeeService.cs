using Auth.Data;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class ApplicableFeeService : IApplicableFeeService
    {
        private readonly ApplicationDbContext _context;
        public ApplicableFeeService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> CreateApplicableFee(Model.DTOs.ApplicableFeeDto applicableFeeDto)
        {

                // Create new applicable fee
                var newApplicableFee = new Model.Entities.ApplicableFee
                {
                    StudentId = applicableFeeDto.StudentId,
                    FeeTypeId = applicableFeeDto.FeeTypeId,
                    Status= FeeStatus.Pending,
                };
                await _context.ApplicableFees.AddAsync(newApplicableFee);
                await _context.SaveChangesAsync();
                return new OkObjectResult(new { message = "Applicable fee created successfully", IsSucceed = true });
            

        }


        public async Task<IActionResult> GetAllApplicableFees()
        {
            var applicableFees = await _context.ApplicableFees.Include(af => af.Student).Include(af => af.FeeType).Select(af => new Model.DTOs.ApplicableFeeWithDetailsDto
            {
                AfId = af.AfId,
                StudentName = af.Student.User.FullName,
                FeeTypeName = af.FeeType.Name,
                Amount = af.FeeType.Amount,
                CreatedAt = af.CreatedAt,
                Status=af.Status,
            }).ToListAsync();
            return new OkObjectResult(applicableFees);
        }

        public async Task<IActionResult> GetApplicableFeesById(Guid id)
        {
            var applicableFee = await _context.ApplicableFees.Include(af => af.Student).Include(af => af.FeeType).Where(af => af.AfId == id).Select(af => new Model.DTOs.ApplicableFeeWithDetailsDto
            {
                AfId = af.AfId,
                StudentName = af.Student.User.FullName,
                FeeTypeName = af.FeeType.Name,
                Amount = af.FeeType.Amount,
                AcademicTerm=af.FeeType.AcademicTerm,
                Status = af.Status,
                CreatedAt = af.CreatedAt
            }).FirstOrDefaultAsync();
            if (applicableFee == null)
            {
                return new NotFoundObjectResult(new { message = "Applicable fee not found" });
            }
            return new OkObjectResult(applicableFee);



        }

        public async Task<IActionResult> GetApplicableFeesByStudentId(Guid studentId)
        {
            var applicableFees = await _context.ApplicableFees.Include(af => af.Student).Include(af => af.FeeType).Where(af => af.StudentId == studentId).Select(af => new Model.DTOs.ApplicableFeeWithDetailsDto
            {
                AfId = af.AfId,
                StudentName = af.Student.User.FullName,
                FeeTypeName = af.FeeType.Name,
                AcademicTerm=af.FeeType.AcademicTerm,
                Amount = af.FeeType.Amount,
                Status = af.Status,
                CreatedAt = af.CreatedAt
            }).ToListAsync();
            if (applicableFees == null || applicableFees.Count == 0)
            {
                return new NotFoundObjectResult(new { message = "No applicable fees found for the student" });
            }
            return new OkObjectResult(applicableFees);
        }

        public async Task<IActionResult> UpdateApplicableFee(Guid id, Model.DTOs.ApplicableFeeDto applicableFeeDto)
        {
            var existingApplicableFee = await _context.ApplicableFees.FindAsync(id);
            if (existingApplicableFee == null)
            {
                return new NotFoundObjectResult(new { message = "Applicable fee not found" });
            }
            // Check if the new combination of StudentId and FeeTypeId already exists
            var duplicateApplicableFee = await _context.ApplicableFees.FirstOrDefaultAsync(af => af.StudentId == applicableFeeDto.StudentId && af.FeeTypeId == applicableFeeDto.FeeTypeId && af.AfId != id);
            if (duplicateApplicableFee != null)
            {
                return new BadRequestObjectResult(new { message = "Another applicable fee with the same StudentId and FeeTypeId already exists" });
            }
            existingApplicableFee.StudentId = applicableFeeDto.StudentId;
            existingApplicableFee.FeeTypeId = applicableFeeDto.FeeTypeId;
            existingApplicableFee.Status = FeeStatus.Pending; // Reset status to Pending when updating
            _context.ApplicableFees.Update(existingApplicableFee);
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Applicable fee updated successfully", IsSucceed = true });

        }

    }
 }
