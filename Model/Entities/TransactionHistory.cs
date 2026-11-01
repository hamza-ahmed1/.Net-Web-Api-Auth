using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public enum PaymentMode { Cash, Card, BankTransfer, JazzCash, EasyPaisa }

    public enum TransactionKind { Payment, Reversal }

    public class TransactionHistory
    {
        [Key]
        public Guid TransactionId { get; set; }

        public Guid InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        /// <summary>Positive for a payment, negative for a reversal.</summary>
        public decimal Amount { get; set; }
        public PaymentMode Mode { get; set; }

        [StringLength(100)]
        public string TransactionReference { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        public TransactionKind Kind { get; set; } = TransactionKind.Payment;

        /// <summary>On a reversal, the payment it cancels.</summary>
        public Guid? ReversesTransactionId { get; set; }

        [StringLength(300)]
        public string? Reason { get; set; }

        public string? RecordedByUserId { get; set; }
    }
}
