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