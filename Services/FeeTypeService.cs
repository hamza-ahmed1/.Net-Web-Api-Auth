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

        private async Task<string?> FindProblemAsync(FeeTypeCreateDto feeType, Guid? ignoring = null)
        {
            if (!FeeRules.HasValidPrecision(feeType.Amount))
                return "Amount can have at most 2 decimal places.";
            if (!await _context.FeeCategories.AnyAsync(c => c.FeeCategoryId == feeType.FeeCategoryId))
                return "Choose a valid fee category.";

            var name = feeType.Name.Trim();
            var duplicate = await _context.FeeTypes.AnyAsync(ft =>
                ft.Name == name && ft.FeeCategoryId == feeType.FeeCategoryId && ft.FeeTypeId != ignoring);
            return duplicate ? "Fee type already exists" : null;
        }

        public async Task<IActionResult> CreateFeeType(FeeTypeCreateDto feeType)
        {
            var problem = await FindProblemAsync(feeType);
            if (problem != null)
                return new BadRequestObjectResult(new { message = problem });

            await _context.FeeTypes.AddAsync(new Model.Entities.FeeType
            {
                FeeCategoryId = feeType.FeeCategoryId,
                Name = feeType.Name.Trim(),
                Amount = feeType.Amount,
                Per = feeType.Per,
                AcademicTerm = feeType.AcademicTerm.Trim(),
                Currency = feeType.Currency,
                ApplicableDate = feeType.ApplicableDate,
                IsActive = feeType.IsActive
            });
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Fee type created successfully", IsSucceed=true });
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
                IsActive = ft.IsActive
            }).ToListAsync();
            return new OkObjectResult(feeTypes);
        }

        public async Task<IActionResult> GetFeeType(Guid feeTypeId)
        {
            var feeType = await _context.FeeTypes.FindAsync(feeTypeId);
            if (feeType == null)
            {
                return new NotFoundObjectResult(new { message = "Fee type not found" });
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
                IsActive = feeType.IsActive
            };
            return new OkObjectResult(feeTypeDetails);
        }

        //update. Invoices keep the amount they were billed at, so this only affects new invoices.
        public async Task<IActionResult> UpdateFeetype(Guid feeTypeId, FeeTypeUpdateDto feeType)
        {
            var existingFeeType = await _context.FeeTypes.FindAsync(feeTypeId);
            if (existingFeeType == null)
            {
                return new NotFoundObjectResult(new { message = "Fee type not found" });
            }

            var problem = await FindProblemAsync(feeType, feeTypeId);
            if (problem != null)
                return new BadRequestObjectResult(new { message = problem });

            existingFeeType.FeeCategoryId = feeType.FeeCategoryId;
            existingFeeType.Name = feeType.Name.Trim();
            existingFeeType.Amount = feeType.Amount;
            existingFeeType.Per = feeType.Per;
            existingFeeType.AcademicTerm = feeType.AcademicTerm.Trim();
            existingFeeType.Currency = feeType.Currency;
            existingFeeType.ApplicableDate = feeType.ApplicableDate;
            existingFeeType.IsActive = feeType.IsActive;
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Fee type updated successfully" });
        }
    }
}
