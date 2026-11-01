using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public enum AdjustmentType { Discount, Scholarship, LateFee }

    /// <summary>A change to what a student owes on one invoice, with the reason and who made it.</summary>
    public class InvoiceAdjustment
    {
        [Key]
        public Guid Id { get; set; }

        public Guid InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        public AdjustmentType Type { get; set; }

        /// <summary>Always positive; Type decides whether it adds to or reduces the amount owed.</summary>
        public decimal Amount { get; set; }

        [Required]
        [StringLength(300)]
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RecordedByUserId { get; set; }

        // Removing keeps the row, so the ledger can still show that it existed
        public bool IsRemoved { get; set; }
        public DateTime? RemovedAt { get; set; }
        public string? RemovedByUserId { get; set; }
        [StringLength(300)]
        public string? RemoveReason { get; set; }
    }
}
