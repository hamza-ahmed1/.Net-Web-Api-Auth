using Auth.Model.DTOs.Section;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.HOD
{
    [Route("api/sections")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;
        public SectionController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSection([FromBody] CreateSectionDTO sectionDto)
        {
            try
            {
                var result = await _sectionService.CreateSection(sectionDto);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while creating the section: {ex.Message}");
            }
        }

        [HttpGet]

        public async Task<List<Section>> GetAll()
        {
            try
            {
                return await _sectionService.GetAllSections();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetByID(Guid Id)
        {
            var section = await _sectionService.GetSectionById(Id);

            if (section == null)
            {
                return NotFound("Section not found.");
            }

            return Ok(section);
        }
        [HttpDelete("{Id}")]

        public async Task<IActionResult> DeleteSec(Guid Id)
        {
            try
            {
                return await _sectionService.DeleteSection(Id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdataCourse(Guid Id, [FromBody] CreateSectionDTO dto)
        {
            try
            {
                await _sectionService.UpdateSection(Id, dto);
                return Ok("Updated");
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
