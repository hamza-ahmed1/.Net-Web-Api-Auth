namespace Auth.Model.Entities
{
    public class FeeType
    {
        public Guid FeeTypeId { get; set; }

        public Guid FeeCategoryId { get; set; }
        public FeeCategory FeeCategory { get; set; }

        public string Name { get; set; }          // "Xi-standard", "Examination Fee"
        public decimal Amount { get; set; }
        public string Per { get; set; }            // "month", "year", "one-time"
        public string AcademicTerm { get; set; }    // "Fall-2026" — prevents recurring-fee ambiguity
        public string Currency { get; set; } = "PKR";
        public DateTime ApplicableDate { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ApplicableFee> ApplicableFees { get; set; }
    }
}
