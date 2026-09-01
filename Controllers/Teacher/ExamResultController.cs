using Auth.Model.DTOs.ExamResult;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Teacher
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamResultController : ControllerBase
    {
        private readonly IExamResultService _examResultService;

        public ExamResultController(IExamResultService examResultService)
        {
            _examResultService = examResultService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateExamResult([FromBody] ExamResultCreateDto dto)
        {
            try
            {
                var result = await _examResultService.CreateExamResult(dto);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateExamResultsBulk([FromBody] List<ExamResultCreateDto> dtos)
        {
            try
            {
                var result = await _examResultService.UploadBulkResult(dtos);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("bulk")]
        public async Task<IActionResult> UpdateExamResultsBulk([FromBody] List<ExamResultCreateDto> dtos)
        {
            try
            {
                var result = await _examResultService.UpdateBulkResult(dtos);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{examResultId}")]
        public async Task<IActionResult> UpdateExamResult(Guid examResultId, [FromBody] ExamResultCreateDto dto)
        {
            try
            {
                var result = await _examResultService.UpdateExamResult(examResultId, dto);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{examResultId}")]
        public async Task<IActionResult> GetExamResultById(Guid examResultId)
        {
            try
            {
                var result = await _examResultService.GetExamResultById(examResultId);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("student={studentId}")]
        public async Task<IActionResult> GetExamsByStudentId(Guid studentId)
        {
            try
            {
                var result = await _examResultService.GetExamsByStudentId(studentId);
                return result;

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("{examResultId}")]
        public async Task<IActionResult> DeleteExamResultById(Guid examResultId)
        {
            try
            {
                await _examResultService.DeleteExamResultById(examResultId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }




    }
}
