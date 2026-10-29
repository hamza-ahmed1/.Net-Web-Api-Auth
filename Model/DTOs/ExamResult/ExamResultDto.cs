using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs.ExamResult
{
    public class ExamResultDto
    {
        public Guid ExamResultId { get; set; }

        public Guid ExamId { get; set; }

        public Guid StudentId { get; set; }

        public decimal? ObtainMarks { get; set; }

        public bool IsAbsent { get; set; }

        public string Remarks { get; set; } = string.Empty;

        // Additional display information
        public string ExamTitle { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;
    }

    public class ExamResultCreateDto
    {
        [Required]
        public Guid ExamId { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ObtainMarks { get; set; }

        public bool IsAbsent { get; set; } = false;

        public string Remarks { get; set; } = string.Empty;
    }
}
namespace Auth.Model.DTOs.ExamResult
{
    /// <summary>
    /// One exam result as shown to a student. For unpublished exams viewed by a student the marks and
    /// remarks are withheld (null) by the server, not just hidden in the app.
    /// </summary>
    public class StudentExamResultDto
    {
        public Guid ExamResultId { get; set; }
        public Guid ExamId { get; set; }
        public Guid StudentId { get; set; }
        public decimal? ObtainMarks { get; set; }
        public bool? IsAbsent { get; set; }
        public string? Remarks { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsPublished { get; set; }
        public StudentExamInfoDto Exam { get; set; } = new();
    }

    public class StudentExamInfoDto
    {
        public Guid ExamID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ExamType { get; set; }
        public int TotalMarks { get; set; }
        public bool IsPublished { get; set; }
        public DateOnly ExamDate { get; set; }
        public Guid TeacherSectionCourseId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
    }
}
