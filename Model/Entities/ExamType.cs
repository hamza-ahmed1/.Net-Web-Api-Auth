using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public class ExamType
    {
        [Key]
        public Guid ExamTypeId { get; set; }

        public string Type { get; set; } = string.Empty;
    }
}
