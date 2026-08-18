using Auth.Data;
using Auth.Model.DTOs.Attendance;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ApplicationDbContext _context;

        public AttendanceController(IAttendanceService attendanceService, ApplicationDbContext context)
        {
            _attendanceService = attendanceService;
            _context = context;
        }

        // TODO: confirm Teacher's real FK to ApplicationUser (assumed: Teacher.UserId)
        // TODO: confirm actual JWT claim name for logged-in user id (assumed: "sub")
        private async Task<Guid?> GetCurrentTeacherId()
        {
            if (User?.Identity?.IsAuthenticated != true)
                return null;

            var identityUserId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(identityUserId))
                return null;

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == identityUserId);

            return teacher?.Teacher_Id;
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> MarkBulkAttendance([FromBody] BulkMarkAttendanceDto dto)
        {
            var teacherId = await GetCurrentTeacherId();
            return await _attendanceService.MarkBulkAttendance(dto, teacherId);
        }

        [HttpPost]
        public async Task<IActionResult> MarkSingleAttendance([FromBody] MarkAttendanceDto dto)
        {
            var teacherId = await GetCurrentTeacherId();
            return await _attendanceService.MarkSingleAttendance(dto, teacherId);
        }

        [HttpGet("{attendanceId}")]
        public async Task<IActionResult> GetAttendanceById(Guid attendanceId)
        {
            var record = await _attendanceService.GetAttendanceById(attendanceId);
            return record == null ? NotFound("Attendance record not found.") : Ok(record);
        }

        [HttpGet("course/{teacherSectionCourseId}")]
        public async Task<IActionResult> GetCourseAttendanceByDate(Guid teacherSectionCourseId, [FromQuery] DateTime date)
        {
            return Ok(await _attendanceService.GetCourseAttendanceByDate(teacherSectionCourseId, date));
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentAttendanceHistory(Guid studentId, [FromQuery] Guid? courseId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            return Ok(await _attendanceService.GetStudentAttendanceHistory(studentId, courseId, from, to));
        }

        [HttpGet("student/{studentId}/courses")]
        public async Task<IActionResult> GetActiveCoursesForStudent(Guid studentId)
        {
            return Ok(await _attendanceService.GetActiveCoursesForStudent(studentId));
        }

        [HttpPut("{attendanceId}")]
        public async Task<IActionResult> UpdateAttendance(Guid attendanceId, [FromBody] UpdateAttendanceDto dto)
        {
            var teacherId = await GetCurrentTeacherId();
            return await _attendanceService.UpdateAttendance(attendanceId, dto, teacherId);
        }

        [HttpDelete("{attendanceId}")]
        public async Task<IActionResult> DeleteAttendance(Guid attendanceId)
        {
            return await _attendanceService.DeleteAttendance(attendanceId);
        }

        [HttpGet("course/{teacherSectionCourseId}/summary")]
        public async Task<IActionResult> GetCourseSummary(Guid teacherSectionCourseId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            return Ok(await _attendanceService.GetCourseSummary(teacherSectionCourseId, from, to));
        }
    }
}