using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Leave
    }

    public class Attendance
    {
        [Key]
        public Guid AttendanceId { get; set; }

        [Required]
        [ForeignKey(nameof(StudentEnrollment))]
        public Guid StudentEnrollmentId { get; set; }
        public StudentEnrollments StudentEnrollment { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(TeacherSectionCourse))]
        public Guid TeacherSectionCourseId { get; set; }
        public TeacherSectionCourse TeacherSectionCourse { get; set; } = null!;

        [Required]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        public string? Remarks { get; set; }

        public Guid? MarkedByTeacherId { get; set; }

        [ForeignKey(nameof(MarkedByTeacherId))]
        public Teacher? MarkedByTeacher { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}