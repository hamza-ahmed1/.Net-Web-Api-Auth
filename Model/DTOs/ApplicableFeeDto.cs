using Auth.Model.Entities;

namespace Auth.Model.DTOs
{
    public class ApplicableFeeDto
    {
        public Guid StudentId { get; set; }
        public Guid FeeTypeId { get; set; }
    }

    public class ApplicableFeeResponseDto
    {
        public Guid AfId { get; set; }
        public Guid StudentId { get; set; }
        public Guid FeeTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApplicableFeeWithDetailsDto
    {
        public Guid AfId { get; set; }
        public string StudentName { get; set; }
        public string FeeTypeName { get; set; }

        public decimal Amount { get; set; } = 0;

        public string AcademicTerm { get; set; } = string.Empty;

        public FeeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    
}
