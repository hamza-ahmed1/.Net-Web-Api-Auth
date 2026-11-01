using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
    public class Invoice
    {
        [Key]
        public Guid Id { get; set; }
        public string InvoiceNum { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public Student? Student { get; set; }

        public Guid FeeTypeId { get; set; } = Guid.Empty;
        public string FeeTypeName { get; set; }=string.Empty;
        public FeeType? feeType { get; set; }

        /// <summary>The fee as billed. Discounts and late fees never change it; they live in the two totals below.</summary>
        public decimal Amount { get; set; }

        /// <summary>Total of the active discounts and scholarships in Adjustments.</summary>
        public decimal Discount { get; set; }

        /// <summary>Total of the active late fees in Adjustments.</summary>
        public decimal LateFee { get; set; }

        /// <summary>Always the sum of this invoice's transactions (a reversal is a negative transaction).</summary>
        public decimal AmountPaid { get; set; }

        [NotMapped]
        public decimal NetAmount => Amount + LateFee - Discount;

        [NotMapped]
        public decimal AmountDue => NetAmount - AmountPaid;

        [NotMapped]
        public bool Ispaid => !IsVoided && AmountDue <= 0;

        public string Month { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Currency { get; set; } = "PKR";
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid UserId { get; set; }

        // Invoices are cancelled, never deleted, so the financial history stays complete
        public bool IsVoided { get; set; }
        public DateTime? VoidedAt { get; set; }
        public string? VoidedByUserId { get; set; }
        [StringLength(300)]
        public string? VoidReason { get; set; }

        /// <summary>Stops two people changing the same invoice at once from overwriting each other.</summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public ICollection<InvoiceAdjustment> Adjustments { get; set; } = new List<InvoiceAdjustment>();
    }
}
