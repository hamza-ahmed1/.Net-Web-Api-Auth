using Auth.Data;
using Auth.Model.DTOs;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Auth.Services
{
    public class FeeCategoryService:IFeeCategoryService
    {
        private readonly ApplicationDbContext _context;
        public FeeCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> CreateCategory(FeeCategoryDto cat)
        {
            var name = cat.categoryName.Trim();
            if (await _context.FeeCategories.AnyAsync(c => c.Name == name))
            {
                return new BadRequestObjectResult(new { message = "Category already exists" });
            }

            await _context.FeeCategories.AddAsync(new Model.Entities.FeeCategory { Name = name });
            await _context.SaveChangesAsync();
            return new OkObjectResult(cat);
        }

        public async Task<IActionResult> UpdateCategory(Guid categoryId, FeeCategoryDto cat)
        {
            var category = await _context.FeeCategories.FindAsync(categoryId);
            if (category == null)
            {
                return new NotFoundObjectResult(new { message = "Category not found" });
            }

            var name = cat.categoryName.Trim();
            if (await _context.FeeCategories.AnyAsync(c => c.Name == name && c.FeeCategoryId != categoryId))
            {
                return new BadRequestObjectResult(new { message = "Category already exists" });
            }

            category.Name = name;
            await _context.SaveChangesAsync();
            return new OkObjectResult(cat);
        }

        public async Task<IActionResult> GetAllCategories()
        {
                var categories = await _context.FeeCategories.Select(c => new FeeCategoryDetailsDto
                {
                    categoryId = c.FeeCategoryId,
                    categoryName = c.Name
                }).ToListAsync();

            return new OkObjectResult(categories);
        }

        public async Task<IActionResult> GetCategory(Guid categoryId)
        {
            var category = await _context.FeeCategories.FindAsync(categoryId);
            if (category == null)
            {
                return new NotFoundObjectResult(new { message = "Category not found" });
            }
            return new OkObjectResult(new FeeCategoryDetailsDto { categoryId = category.FeeCategoryId, categoryName = category.Name });
        }
    }
}
