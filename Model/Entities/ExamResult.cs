using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public class ExamResult
    {
        [Key]
        public Guid ExamResultId { get; set; }

        public Guid ExamId { get; set; }
        public Exam Exam { get; set; }

        public Guid StudentId { get; set; }
        public Student Student { get; set; }


        [Range(0, double.MaxValue)]
        public decimal? ObtainMarks { get; set; }
        public bool IsAbsent { get; set; } = false;

        public string Remarks { get; set; } = string.Empty;

        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }
}
