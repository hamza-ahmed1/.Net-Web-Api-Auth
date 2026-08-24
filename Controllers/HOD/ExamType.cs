using Auth.Model.DTOs;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.HOD
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamType : ControllerBase
    {
        private readonly IExamTypeService _examTypeService;
        public ExamType(IExamTypeService examTypeService)
        {
            _examTypeService = examTypeService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateExamType([FromBody] ExamTypeDTO dto)
        {
            try
            {
                return await _examTypeService.CreateExamType(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExamType(Guid id, [FromBody] ExamTypeDTO dto)
        {
            try
            {
                return await _examTypeService.UpdateExamType(id, dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return await _examTypeService.GetAll();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
