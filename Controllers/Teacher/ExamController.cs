using System.Security.Claims;
using Auth.Model.DTOs.Exam;
using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Teacher
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,HOD,Teacher")]
    public class ExamController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly TeachingAccessService _access;

        public ExamController(IExamService examService, TeachingAccessService access)
        {
            _examService = examService;
            _access = access;
        }

        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        private IActionResult NotYourCourse() =>
            StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only manage exams for courses you teach." });

        [HttpPost]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDto examDto)
        {
            if (!await _access.OwnsAssignmentAsync(CurrentUserId, examDto.TeacherSectionCourseId))
                return NotYourCourse();

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
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> UpdateExam(Guid examId, [FromBody] CreateExamDto examDto)
        {
            // Must own the exam now and the assignment it is moved to
            if (!await _access.OwnsExamAsync(CurrentUserId, examId) || !await _access.OwnsAssignmentAsync(CurrentUserId, examDto.TeacherSectionCourseId))
                return NotYourCourse();

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

        /// <summary>Publishes or unpublishes an exam's results. Only the teacher of that course-section may do this.</summary>
        [HttpPut("{examId:guid}/publish")]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> PublishExam(Guid examId, [FromBody] PublishExamDto dto)
        {
            if (!await _access.OwnsExamAsync(CurrentUserId, examId))
                return NotYourCourse();

            return await _examService.SetPublishedAsync(examId, dto.IsPublished);
        }

        [HttpDelete("{examId}")]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> DeleteExam(Guid examId)
        {
            if (!await _access.OwnsExamAsync(CurrentUserId, examId))
                return NotYourCourse();

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


        [HttpGet("teacher/{teacherId}")]
            public async Task<IActionResult> GetAllExambyTeacherId(Guid teacherId)
        {
            try
            {
                var result = await _examService.GetExamByTeacherId(teacherId);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }



    }
}
