using Auth.Model.Entities;

namespace Auth.Model.DTOs
{
    public class InvoiceDto
    {
    }

    public class GenerateInvoiceRequest
    {
        public List<ApplicableFee> Fees { get; set; }

        public string Month { get; set; }
        public string Year { get; set; }
        public DateTime DueDate { get; set; }
    }


    public class InvoiceDetailsDto
    {
        public Guid Id { get; set; }
        public string InvoiceNum { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid FeeTypeId { get; set; } = Guid.Empty;
        public string FeeTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue => Amount - AmountPaid;
        public bool Ispaid => AmountDue <= 0;
        public string Month { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Currency { get; set; } = "PKR";
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
