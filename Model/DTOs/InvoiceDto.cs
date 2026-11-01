using System.ComponentModel.DataAnnotations;
using Auth.Model.Entities;
using Auth.Services;

namespace Auth.Model.DTOs
{
    public class InvoiceFeeItem
    {
        public Guid StudentId { get; set; }
        public Guid FeeTypeId { get; set; }
    }

    public class GenerateInvoiceRequest
    {
        [Required(ErrorMessage = "Select at least one fee.")]
        public List<InvoiceFeeItem> Fees { get; set; } = new();

        [Required(ErrorMessage = "Month is required.")]
        public string Month { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required.")]
        public string Year { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }
    }

    public class InvoiceDetailsDto
    {
        public Guid Id { get; set; }
        public string InvoiceNum { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string? Section { get; set; }
        public Guid FeeTypeId { get; set; } = Guid.Empty;
        public string FeeTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal LateFee { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal NetAmount => Amount + LateFee - Discount;
        public decimal AmountDue => IsVoided ? 0 : NetAmount - AmountPaid;
        public bool Ispaid => !IsVoided && AmountDue <= 0;
        public string Status => FeeRules.InvoiceStatus(NetAmount, AmountPaid, DueDate, IsVoided);
        public string Month { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Currency { get; set; } = "PKR";
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsVoided { get; set; }
        public string? VoidReason { get; set; }
    }

    public class ReasonDto
    {
        [Required(ErrorMessage = "Enter a reason.")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "Enter a reason of 3 to 300 characters.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class AddAdjustmentDto : ReasonDto
    {
        public AdjustmentType Type { get; set; }

        [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Amount must be more than 0.")]
        public decimal Amount { get; set; }
    }

    public class AdjustmentDetailsDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? RecordedBy { get; set; }
        public bool IsRemoved { get; set; }
        public string? RemoveReason { get; set; }
    }

    public record SkippedFee(Guid StudentId, string Student, string Fee, string Reason);

    public class FeePage<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
