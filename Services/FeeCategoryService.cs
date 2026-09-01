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
            var existingCategory = await _context.FeeCategories.FirstOrDefaultAsync(c => c.Name == cat.categoryName);
            if (existingCategory != null)
            {
                return new BadRequestObjectResult("Category already exists");
            }
            var category = new Model.Entities.FeeCategory
            {
                Name = cat.categoryName
            };

            await _context.FeeCategories.AddAsync(category);
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
                return new NotFoundObjectResult("Category not found");
            }
            return new OkObjectResult(category);
        }
    }
}
