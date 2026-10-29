using System.Security.Claims;
using Auth.Data;
using Auth.Model.DTOs.Attendance;
using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Controllers.Teacher
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ApplicationDbContext _context;
        private readonly TeachingAccessService _access;

        public AttendanceController(IAttendanceService attendanceService, ApplicationDbContext context, TeachingAccessService access)
        {
            _attendanceService = attendanceService;
            _context = context;
            _access = access;
        }

        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        private bool IsStaff => User.IsInRole("Admin") || User.IsInRole("HOD") || User.IsInRole("Teacher");

        private IActionResult NotYourCourse() =>
            StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only manage attendance for courses you teach." });

        private async Task<Guid?> GetCurrentTeacherId()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return null;

            // Tokens carry the user id as NameIdentifier (TokenService), not "sub"
            var identityUserId = CurrentUserId;
            if (string.IsNullOrEmpty(identityUserId))
                return null;

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == identityUserId);

            return teacher?.Teacher_Id;
        }

        /// <summary>Students may only read their own attendance; staff may read anyone's.</summary>
        private async Task<bool> CanReadStudentAsync(Guid studentId)
        {
            return IsStaff || await _access.StudentIdForUserAsync(CurrentUserId) == studentId;
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> MarkBulkAttendance([FromBody] BulkMarkAttendanceDto dto)
        {
            if (!await _access.OwnsAssignmentAsync(CurrentUserId, dto.TeacherSectionCourseId))
                return NotYourCourse();

            var teacherId = await GetCurrentTeacherId();
            return await _attendanceService.MarkBulkAttendance(dto, teacherId);
        }

        [HttpPost]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> MarkSingleAttendance([FromBody] MarkAttendanceDto dto)
        {
            if (!await _access.OwnsAssignmentAsync(CurrentUserId, dto.TeacherSectionCourseId))
                return NotYourCourse();

            var teacherId = await GetCurrentTeacherId();
            return await _attendanceService.MarkSingleAttendance(dto, teacherId);
        }

        [HttpGet("{attendanceId}")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetAttendanceById(Guid attendanceId)
        {
            var record = await _attendanceService.GetAttendanceById(attendanceId);
            return record == null ? NotFound("Attendance record not found.") : Ok(record);
        }

        [HttpGet("course/{teacherSectionCourseId}")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetCourseAttendanceByDate(Guid teacherSectionCourseId, [FromQuery] DateTime date)
        {
            return Ok(await _attendanceService.GetCourseAttendanceByDate(teacherSectionCourseId, date));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentAttendanceHistory(Guid studentId, [FromQuery] Guid? courseId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!await CanReadStudentAsync(studentId))
                return Forbid();

            return Ok(await _attendanceService.GetStudentAttendanceHistory(studentId, courseId, from, to));
        }

        [HttpGet("student/{studentId}/courses")]
        public async Task<IActionResult> GetActiveCoursesForStudent(Guid studentId)
        {
            if (!await CanReadStudentAsync(studentId))
                return Forbid();

            return Ok(await _attendanceService.GetActiveCoursesForStudent(studentId));
        }

        [HttpPut("{attendanceId}")]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> UpdateAttendance(Guid attendanceId, [FromBody] UpdateAttendanceDto dto)
        {
            if (!await _access.OwnsAttendanceAsync(CurrentUserId, attendanceId))
                return NotYourCourse();

            var teacherId = await GetCurrentTeacherId();
            return await _attendanceService.UpdateAttendance(attendanceId, dto, teacherId);
        }

        [HttpDelete("{attendanceId}")]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> DeleteAttendance(Guid attendanceId)
        {
            if (!await _access.OwnsAttendanceAsync(CurrentUserId, attendanceId))
                return NotYourCourse();

            return await _attendanceService.DeleteAttendance(attendanceId);
        }

        [HttpGet("course/{teacherSectionCourseId}/summary")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetCourseSummary(Guid teacherSectionCourseId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            return Ok(await _attendanceService.GetCourseSummary(teacherSectionCourseId, from, to));
        }
    }
}
