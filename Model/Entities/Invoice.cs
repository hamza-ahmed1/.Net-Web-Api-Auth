using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
    public class Invoice
    {
        public long Id { get; set; }

        // Persist InvoiceNum in the database (index is defined in the model/migrations).
        // Keep a backing field so EF can map the property while still returning a computed
        // fallback when the backing field is null.
        private string _invoiceNum;
        public string InvoiceNum
        {
            get => _invoiceNum ?? $"INV-{Id:D6}";
            set => _invoiceNum = value;
        }

        public Guid StudentId { get; set; }
        public Student Student { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal AmountPaid { get; set; }

        [NotMapped]
        public decimal AmountDue => TotalAmount - AmountPaid;

        public string Currency { get; set; } = "PKR";

        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }
    }
}