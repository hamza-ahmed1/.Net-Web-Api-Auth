using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = FeeRules.Staff)]
    public class FeeCategoryController : ControllerBase
    {
        private readonly IFeeCategoryService _feeCategoryService;
        public FeeCategoryController(IFeeCategoryService feeCategoryService)
        {
            _feeCategoryService = feeCategoryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Model.DTOs.FeeCategoryDto feeCategory)
        {
            return await _feeCategoryService.CreateCategory(feeCategory);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] Model.DTOs.FeeCategoryDto feeCategory)
        {
            return await _feeCategoryService.UpdateCategory(id, feeCategory);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            return await _feeCategoryService.GetAllCategories();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(Guid id)
        {
            return await _feeCategoryService.GetCategory(id);
        }
    }
}
