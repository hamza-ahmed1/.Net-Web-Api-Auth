using System.ComponentModel.DataAnnotations;
using Auth.Model.Entities;

namespace Auth.Model.DTOs
{
    public class TransactionDto
    {
        public Guid invoiceID { get; set; }

        [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Amount must be more than 0.")]
        public decimal Amount { get; set; }

        public PaymentMode Mode { get; set; }

        /// <summary>Must be unique, so submitting the same payment twice records it once.</summary>
        [Required(ErrorMessage = "Enter a transaction reference.")]
        [StringLength(100, ErrorMessage = "Reference can be at most 100 characters.")]
        public string TransactionRef { get; set; } = string.Empty;
    }

    public class TransactionDetailsDto
    {
        public Guid TransactionId { get; set; }
        public Guid InvoiceId { get; set; }
        public string InvoiceNum { get; set; } = string.Empty;
        public string FeeTypeName { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PKR";
        public string Mode { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public string? Reason { get; set; }
        public string? RecordedBy { get; set; }
        /// <summary>True when a later reversal has cancelled this payment.</summary>
        public bool IsReversed { get; set; }
    }

    public class TransactionQuery
    {
        public string? Search { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public PaymentMode? Mode { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ReceiptDto
    {
        public string ReceiptNo { get; set; } = string.Empty;
        public TransactionDetailsDto Transaction { get; set; } = new();
        public string StudentCode { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string Period { get; set; } = string.Empty;
        public decimal InvoiceNet { get; set; }
        public decimal InvoicePaid { get; set; }
        public decimal BalanceDue { get; set; }
    }
}
