using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public class Course
    {
        [Key]
        public Guid CourseId { get; set; }
        [Required]
        public string CourseName { get; set; } = string.Empty;
        [Required]
        public string CourseDescription { get; set; } = string.Empty;
        [Required]
        public int CourseDuration { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<TeacherSectionCourse> TeacherAssignments { get; set; }
    = new List<TeacherSectionCourse>();
    }
}
