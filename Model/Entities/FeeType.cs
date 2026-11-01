using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public class FeeType
    {
        public Guid FeeTypeId { get; set; }

        public Guid FeeCategoryId { get; set; }
        public FeeCategory FeeCategory { get; set; }

        [StringLength(150)]
        public string Name { get; set; }          // "Xi-standard", "Examination Fee"
        public decimal Amount { get; set; }
        [StringLength(20)]
        public string Per { get; set; }            // "month", "year", "one-time"
        [StringLength(50)]
        public string AcademicTerm { get; set; }    // "Fall-2026" — prevents recurring-fee ambiguity
        [StringLength(3)]
        public string Currency { get; set; } = "PKR";
        public DateTime ApplicableDate { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ApplicableFee> ApplicableFees { get; set; }
    }
}
