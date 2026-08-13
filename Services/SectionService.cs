using Auth.Data;
using Auth.Model.DTOs.Section;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class SectionService : ISectionService
    {
        private readonly ApplicationDbContext _context;
        public SectionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Removed 'new' so this implements the interface method
        public async Task<IActionResult> CreateSection(CreateSectionDTO sectionDto)
        {

            // check if already exists:
            var existingSection = await _context.Sections
                .FirstOrDefaultAsync(s => s.SectionName == sectionDto.SectionName && s.IntermediateClass == sectionDto.IntermediateClass);
            if (existingSection != null) {
                return new BadRequestObjectResult("Section already exists.");
            }

            var section = new Section
            {
               SectionName = sectionDto.SectionName,
               IntermediateClass = sectionDto.IntermediateClass,
               StartDate = sectionDto.StartDate,
               IsActive = sectionDto.IsActive
            };

            await _context.Sections.AddAsync(section);
            await _context.SaveChangesAsync();
            return new OkObjectResult("Section has been created");
        }

        // Implemented missing interface members as stubs
        public async Task<List<Section>> GetAllSections()
        {
            return await _context.Sections.ToListAsync();
        }

        public async Task<Section?> GetSectionById(Guid sectionId)
        {
            var section = await _context.Sections.FindAsync(sectionId);

            return section;
        }

        public async Task<IActionResult> UpdateSection(Guid sectionId, CreateSectionDTO sectionDto)
        {
            var section = await _context.Sections.FindAsync(sectionId);
            if (section == null)
            {
                return new NotFoundObjectResult("Section not found.");
            }

            section.SectionName = sectionDto.SectionName;
            section.IntermediateClass = sectionDto.IntermediateClass;
            section.StartDate = sectionDto.StartDate;
            section.IsActive = sectionDto.IsActive;

            await _context.SaveChangesAsync();
            return new OkResult();
        }

        public async Task<IActionResult> DeleteSection(Guid sectionId)
        {
            var section = await _context.Sections.FindAsync(sectionId);
            if (section == null)
            {
                return new NotFoundResult();
            }

            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
            return new OkResult();
        }
    }
}
