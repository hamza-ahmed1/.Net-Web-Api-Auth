using Auth.Model.DTOs.Section;
using Auth.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface ISectionService
    {
        public Task<IActionResult> CreateSection(CreateSectionDTO   sectionDto);
        public Task<List<Section>> GetAllSections();
        public Task<Section> GetSectionById(Guid sectionId);
        public Task<IActionResult> UpdateSection(Guid sectionId, CreateSectionDTO sectionDto);
        public Task<IActionResult> DeleteSection(Guid sectionId);
    }
}
