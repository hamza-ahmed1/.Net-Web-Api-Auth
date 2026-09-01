using Auth.Model.Entities;

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
        public int status { get; set; }
    }

    public class FeeTypeCreateDto
    {
        public Guid FeeCategoryId { get; set; }
        public string Name { get; set; }          // "Xi-standard", "Examination Fee"
        public decimal Amount { get; set; }
        public string Per { get; set; }            // "month", "year", "one-time"
        public string AcademicTerm { get; set; }    // "Fall-2026" — prevents recurring-fee ambiguity
        public string Currency { get; set; } = "PKR";
        public DateTime ApplicableDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class FeeTypeUpdateDto
    {
        public Guid FeeCategoryId { get; set; }
        public string Name { get; set; }          // "Xi-standard", "Examination Fee"
        public decimal Amount { get; set; }
        public string Per { get; set; }            // "month", "year", "one-time"
        public string AcademicTerm { get; set; }    // "Fall-2026" — prevents recurring-fee ambiguity
        public string Currency { get; set; } = "PKR";
        public DateTime ApplicableDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
