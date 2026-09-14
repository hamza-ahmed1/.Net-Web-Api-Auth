using Auth.Model.Entities;

namespace Auth.Model.DTOs
{
    public class TransactionDto
    {
        public Guid invoiceID { get; set; }
        public decimal Amount { get; set; }

        public PaymentMode Mode { get; set; }

        public string TransactionRef { get; set; } = string.Empty;
    }
}
