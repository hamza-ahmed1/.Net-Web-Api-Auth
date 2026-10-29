using System.Security.Claims;
using Auth.Model.DTOs.ExamResult;
using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Teacher
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExamResultController : ControllerBase
    {
        private readonly IExamResultService _examResultService;
        private readonly TeachingAccessService _access;

        public ExamResultController(IExamResultService examResultService, TeachingAccessService access)
        {
            _examResultService = examResultService;
            _access = access;
        }

        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        private IActionResult NotYourExam() =>
            StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only enter results for exams in courses you teach." });

        [HttpPost]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> CreateExamResult([FromBody] ExamResultCreateDto dto)
        {
            if (!await _access.OwnsExamAsync(CurrentUserId, dto.ExamId))
                return NotYourExam();

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
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> CreateExamResultsBulk([FromBody] List<ExamResultCreateDto> dtos)
        {
            if (!await _access.OwnsAllExamsAsync(CurrentUserId, dtos.Select(d => d.ExamId)))
                return NotYourExam();

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
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> UpdateExamResultsBulk([FromBody] List<ExamResultCreateDto> dtos)
        {
            if (!await _access.OwnsAllExamsAsync(CurrentUserId, dtos.Select(d => d.ExamId)))
                return NotYourExam();

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
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> UpdateExamResult(Guid examResultId, [FromBody] ExamResultCreateDto dto)
        {
            if (!await _access.OwnsResultAsync(CurrentUserId, examResultId) || !await _access.OwnsExamAsync(CurrentUserId, dto.ExamId))
                return NotYourExam();

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
        [Authorize(Roles = "Admin,HOD,Teacher")]
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

        /// <summary>
        /// A student's results. Students may only request their own, and get no marks for unpublished exams;
        /// staff see everything.
        /// </summary>
        [HttpGet("student={studentId}")]
        public async Task<IActionResult> GetExamsByStudentId(Guid studentId)
        {
            var isStaff = User.IsInRole("Admin") || User.IsInRole("HOD") || User.IsInRole("Teacher");
            if (!isStaff)
            {
                var ownStudentId = await _access.StudentIdForUserAsync(CurrentUserId);
                if (ownStudentId != studentId)
                    return Forbid();
            }

            try
            {
                var result = await _examResultService.GetExamsByStudentId(studentId, withholdUnpublished: !isStaff);
                return result;

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("{examResultId}")]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> DeleteExamResultById(Guid examResultId)
        {
            if (!await _access.OwnsResultAsync(CurrentUserId, examResultId))
                return NotYourExam();

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

        [HttpGet("exam={examId}")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetExamResultsByExamId(Guid examId)
        {
            try
            {
                var result = await _examResultService.GetExamResultsByExamId(examId);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }




    }
}
