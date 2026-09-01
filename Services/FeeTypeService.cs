using Auth.Data;
using Auth.Model.DTOs;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class FeeTypeService:IFeeTypeService
    {
        private readonly ApplicationDbContext _context;
        public FeeTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CreateFeeType(FeeTypeCreateDto feeType)
        {
            // check if alredy exists
            var existingFeeType = _context.FeeTypes.FirstOrDefault(ft => ft.Name == feeType.Name && ft.FeeCategoryId == feeType.FeeCategoryId);
            if (existingFeeType == null)
            {
                // Create new fee type 
                var newFeeType = new Model.Entities.FeeType
                {
                    FeeCategoryId = feeType.FeeCategoryId,
                    Name = feeType.Name,
                    Amount = feeType.Amount,
                    Per = feeType.Per,
                    AcademicTerm = feeType.AcademicTerm,
                    Currency = feeType.Currency,
                    ApplicableDate = feeType.ApplicableDate,
                    IsActive = feeType.IsActive
                };
                await _context.FeeTypes.AddAsync(newFeeType);
                await _context.SaveChangesAsync();
                return new OkObjectResult(new { message = "Fee type created successfully", IsSucceed=true });



            }
            else
            {
                return new BadRequestObjectResult(new { message = "Fee type already exists" });
            }
        }
            
        public async Task<IActionResult> GetAllFeeTypes()
        {
            var feeTypes = await _context.FeeTypes.Select(ft => new FeeTypeDetailsDto
            {
                FeeTypeId = ft.FeeTypeId,
                FeeCategoryId = ft.FeeCategoryId,
                Name = ft.Name,
                Amount = ft.Amount,
                Per = ft.Per,
                AcademicTerm = ft.AcademicTerm,
                Currency = ft.Currency,
                ApplicableDate = ft.ApplicableDate,
        
            }).ToListAsync();
            return new OkObjectResult(feeTypes);
        }

        public async Task<IActionResult> GetFeeType(Guid feeTypeId)
        {
            var feeType = await _context.FeeTypes.FindAsync(feeTypeId);
            if (feeType == null)
            {
                return new NotFoundObjectResult("Fee type not found");
            }
            var feeTypeDetails = new FeeTypeDetailsDto
            {
                FeeTypeId = feeType.FeeTypeId,
                FeeCategoryId = feeType.FeeCategoryId,
                Name = feeType.Name,
                Amount = feeType.Amount,
                Per = feeType.Per,
                AcademicTerm = feeType.AcademicTerm,
                Currency = feeType.Currency,
                ApplicableDate = feeType.ApplicableDate,
       
            };
            return new OkObjectResult(feeTypeDetails);
        }

        //update
        public async Task<IActionResult> UpdateFeetype(Guid feeTypeId, FeeTypeUpdateDto feeType)
        {
            var existingFeeType = await _context.FeeTypes.FindAsync(feeTypeId);
            if (existingFeeType == null)
            {
                return new NotFoundObjectResult("Fee type not found");
            }
            existingFeeType.FeeCategoryId = feeType.FeeCategoryId;
            existingFeeType.Name = feeType.Name;
            existingFeeType.Amount = feeType.Amount;
            existingFeeType.Per = feeType.Per;
            existingFeeType.AcademicTerm = feeType.AcademicTerm;
            existingFeeType.Currency = feeType.Currency;
            existingFeeType.ApplicableDate = feeType.ApplicableDate;
            existingFeeType.IsActive = feeType.IsActive;
            _context.FeeTypes.Update(existingFeeType);
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Fee type updated successfully" });
        }
    }
}
