using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
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
