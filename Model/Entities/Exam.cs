using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
    public class Exam
    {
        [Key]
        public Guid ExamID { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ExamType { get; set; } = string.Empty;
        [Required]
        public int TotalMarks { get; set; }

        [Required]
        public bool IsPublished { get; set; } = false;

        [Required]
        public DateOnly ExamDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        [ForeignKey(nameof(TeacherSectionCourse))]
        public Guid TeacherSectionCourseId { get; set; }

        public TeacherSectionCourse TeacherSectionCourse { get; set; } = null!;

    }
}
