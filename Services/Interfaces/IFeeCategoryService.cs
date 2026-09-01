using Auth.Model.DTOs;
using Auth.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IFeeCategoryService
    {
        public Task<IActionResult> CreateCategory(FeeCategoryDto feeCategory);
        public Task<IActionResult> GetAllCategories();
        public Task<IActionResult> GetCategory(Guid categoryId);
    }
}
