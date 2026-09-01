using Auth.Model.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IFeeTypeService
    {
        public Task<IActionResult> CreateFeeType(FeeTypeCreateDto feeType);
        public Task<IActionResult> GetAllFeeTypes();
        public Task<IActionResult> GetFeeType(Guid feeCategoryId);

        public Task<IActionResult> UpdateFeetype(Guid feeTypeId, FeeTypeUpdateDto feeType);

    }
}
