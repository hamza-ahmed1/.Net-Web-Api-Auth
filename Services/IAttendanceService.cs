using Auth.Model.DTOs.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<IActionResult> MarkBulkAttendance(BulkMarkAttendanceDto dto, Guid? markedByTeacherId);
        Task<IActionResult> MarkSingleAttendance(MarkAttendanceDto dto, Guid? markedByTeacherId);
        Task<AttendanceDto?> GetAttendanceById(Guid attendanceId);
        Task<List<AttendanceDto>> GetCourseAttendanceByDate(Guid teacherSectionCourseId, DateTime date);
        Task<List<AttendanceDto>> GetStudentAttendanceHistory(Guid studentId, Guid? courseId, DateTime? from, DateTime? to);
        Task<IActionResult> UpdateAttendance(Guid attendanceId, UpdateAttendanceDto dto, Guid? markedByTeacherId);
        Task<IActionResult> DeleteAttendance(Guid attendanceId);
        Task<List<AttendanceSummaryDto>> GetCourseSummary(Guid teacherSectionCourseId, DateTime? from, DateTime? to);
        Task<List<StudentCourseDto>> GetActiveCoursesForStudent(Guid studentId);
    }
}