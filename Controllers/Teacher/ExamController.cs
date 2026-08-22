using Auth.Model.DTOs.Exam;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Teacher
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;
        public ExamController(IExamService examService)
        {
            _examService = examService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDto examDto)
        {
            try
            {
                var result = await _examService.CreateExam(examDto);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");

            }
        }


        [HttpGet("{id}")]

        public async Task<IActionResult> GetAllExamByTeacherSection(Guid id)
        {
            try
            {
                var result = await _examService.GetAllExamByTeacherSection(id);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{examId}")]
        public async Task<IActionResult> UpdateExam(Guid examId, [FromBody] CreateExamDto examDto)
        {
            try
            {
                var result = await _examService.UpdateExam(examId, examDto);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{examId}")]
        public async Task<IActionResult> DeleteExam(Guid examId)
        {
            try
            {
                var result = await _examService.DeleteExam(examId);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

      
    }
}
