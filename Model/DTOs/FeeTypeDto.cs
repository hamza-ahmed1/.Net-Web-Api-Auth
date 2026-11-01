using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs
{
    public class FeeTypeDetailsDto
    {
        public Guid FeeTypeId { get; set; }
        public Guid FeeCategoryId { get; set; }
        public string Name { get; set; }          // "Xi-standard", "Examination Fee"
        public decimal Amount { get; set; }
        public string Per { get; set; }            // "month", "year", "one-time"
        public string AcademicTerm { get; set; }    // "Fall-2026" — prevents recurring-fee ambiguity
        public string Currency { get; set; } = "PKR";
        public DateTime ApplicableDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class FeeTypeCreateDto
    {
        public Guid FeeCategoryId { get; set; }

        [Required(ErrorMessage = "Fee name is required.")]
        [StringLength(150, ErrorMessage = "Fee name can be at most 150 characters.")]
        public string Name { get; set; }          // "Xi-standard", "Examination Fee"

        [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Amount must be more than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Choose how often this fee is charged.")]
        [RegularExpression("^(month|year|one-time)$", ErrorMessage = "A fee is charged per month, per year or one time.")]
        public string Per { get; set; }            // "month", "year", "one-time"

        [Required(ErrorMessage = "Academic term is required.")]
        [StringLength(50, ErrorMessage = "Academic term can be at most 50 characters.")]
        public string AcademicTerm { get; set; }    // "Fall-2026" — prevents recurring-fee ambiguity

        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be a 3-letter code such as PKR.")]
        public string Currency { get; set; } = "PKR";

        public DateTime ApplicableDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class FeeTypeUpdateDto : FeeTypeCreateDto
    {
    }
}
