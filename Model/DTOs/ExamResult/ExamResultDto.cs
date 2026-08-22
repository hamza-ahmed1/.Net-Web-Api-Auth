using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs.ExamResult
{
    public class ExamResultDto
    {

    }
    public class ExamResultCreateDto
    {
        public Guid ExamId { get; set; }
        public Auth.Model.Entities.Exam  Exam{ get; set; }

        public Guid StudentId { get; set; }
        public Auth.Model.Entities.Student Student { get; set; }


        [Range(0, double.MaxValue)]
        public decimal? ObtainMarks { get; set; }
        public bool IsAbsent { get; set; } = false;
        public string Remarks { get; set; } = string.Empty;
    }
}
