using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.DTOs.Exam
{
    public class CreateExamDto
    {
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
        [Required]
        public Guid TeacherSectionCourseId { get; set; }

    }
}
