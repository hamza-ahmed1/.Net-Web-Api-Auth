using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    public enum PaymentMode { Cash, Card, BankTransfer, JazzCash, EasyPaisa }

    public class TransactionHistory
    {
        [Key]
        public Guid TransactionId { get; set; }

        public long InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        public decimal Amount { get; set; }
        public PaymentMode Mode { get; set; }
        public string TransactionReference { get; set; }  
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }
}
