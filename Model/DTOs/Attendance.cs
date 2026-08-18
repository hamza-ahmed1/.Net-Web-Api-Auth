using Auth.Model.Entities;

namespace Auth.Model.DTOs.Attendance
{
    public class BulkMarkAttendanceDto
    {
        public Guid TeacherSectionCourseId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public List<StudentAttendanceEntryDto> Entries { get; set; } = new();
    }

    public class StudentAttendanceEntryDto
    {
        public Guid StudentEnrollmentId { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class MarkAttendanceDto
    {
        public Guid StudentEnrollmentId { get; set; }
        public Guid TeacherSectionCourseId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class UpdateAttendanceDto
    {
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class AttendanceDto
    {
        public Guid AttendanceId { get; set; }
        public Guid StudentEnrollmentId { get; set; }
        public Guid StudentId { get; set; }
        public string? StudentFullName { get; set; }
        public Guid TeacherSectionCourseId { get; set; }
        public Guid SectionId { get; set; }
        public string? SectionName { get; set; }
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public Guid TeacherId { get; set; }
        public Guid? MarkedByTeacherId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class BulkAttendanceResultDto
    {
        public Guid TeacherSectionCourseId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int RecordsCreated { get; set; }
        public int RecordsUpdated { get; set; }
        public int RecordsSkipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class AttendanceSummaryDto
    {
        public Guid StudentId { get; set; }
        public string? StudentFullName { get; set; }
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int LeaveCount { get; set; }
        public double AttendancePercentage { get; set; }
    }

    public class StudentCourseDto
    {
        public Guid TeacherSectionCourseId { get; set; }
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public Guid TeacherId { get; set; }
        public Guid SectionId { get; set; }
        public string? SectionName { get; set; }
    }
}