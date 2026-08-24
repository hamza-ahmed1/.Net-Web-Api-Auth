using Auth.Data;
using Auth.Model.DTOs;
using Auth.Model.DTOs.Exam;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class ExamTypeService:IExamTypeService
    {
        private readonly ApplicationDbContext _context;

        public ExamTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CreateExamType(ExamTypeDTO dto)
        {
            // first check if similar name exists or not:
            var exists = await _context.ExamTypes.FirstOrDefaultAsync(ex => ex.Type == dto.Type);
            if(exists!=null)
            {
                return new BadRequestObjectResult("Same type already exists!");
            }    
            var type = new ExamType
            {
                Type = dto.Type
            };

            await _context.ExamTypes.AddAsync(type);
            await _context.SaveChangesAsync();
            return new OkResult();
        }
        public async Task<IActionResult> UpdateExamType(Guid id,ExamTypeDTO dto)
        {
            // first check if similar name exists or not:
            var exists = await _context.ExamTypes.FirstOrDefaultAsync(ex => ex.Type == dto.Type);
            if (exists != null)
            {
                return new BadRequestObjectResult("Same type already exists!");
            }
            // get exam type:
            var examType = await _context.ExamTypes.FirstOrDefaultAsync(ex => ex.ExamTypeId == id);
            if (examType != null)
            {
                examType.Type = dto.Type;
                await _context.SaveChangesAsync();

                return new OkResult();
            }

            return new BadRequestResult();

        }
        public async Task<IActionResult> GetAll()
        {
            var types = await _context.ExamTypes.ToListAsync();

            return new OkObjectResult(new { types });
        }
    }
}
