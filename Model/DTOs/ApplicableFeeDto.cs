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
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid FeeTypeId { get; set; }
        public string FeeTypeName { get; set; } = string.Empty;

        public decimal Amount { get; set; } = 0;
        public string Currency { get; set; } = "PKR";
        public string Per { get; set; } = string.Empty;

        public string AcademicTerm { get; set; } = string.Empty;

        /// <summary>Pending, Clear or Overdue, worked out from the invoices.</summary>
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
