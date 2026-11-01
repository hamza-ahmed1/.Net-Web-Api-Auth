namespace Auth.Model.DTOs
{
    public class FeeReportQuery
    {
        public string? Month { get; set; }
        public string? Year { get; set; }
        public Guid? SectionId { get; set; }
        /// <summary>Totals cover one currency; empty means the most used one.</summary>
        public string? Currency { get; set; }
    }

    public record FeeReportRow(string Label, int Invoices, decimal Billed, decimal Collected, decimal Outstanding);

    public record PaymentModeRow(string Mode, decimal Amount);

    public class FeeSummaryDto
    {
        public string Currency { get; set; } = "PKR";
        public List<string> Currencies { get; set; } = new();
        public int Invoices { get; set; }
        public decimal Billed { get; set; }
        public decimal Discounts { get; set; }
        public decimal LateFees { get; set; }
        public decimal NetBilled { get; set; }
        public decimal Collected { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Overdue { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public List<FeeReportRow> ByMonth { get; set; } = new();
        public List<FeeReportRow> ByFeeType { get; set; } = new();
        public List<PaymentModeRow> ByMode { get; set; } = new();
    }

    public class DefaulterDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string? Section { get; set; }
        public int Invoices { get; set; }
        public decimal Overdue { get; set; }
        public DateTime OldestDueDate { get; set; }
    }

    public record LedgerEntry(DateTime Date, string Type, string Description, string InvoiceNum, decimal Debit, decimal Credit, decimal Balance);

    public class LedgerDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string Currency { get; set; } = "PKR";
        public List<string> Currencies { get; set; } = new();
        public decimal Balance { get; set; }
        public List<LedgerEntry> Entries { get; set; } = new();
    }

    /// <summary>A student the signed-in fee manager may work with.</summary>
    public class FeeStudentDto
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string? Section { get; set; }
        public Guid? SectionId { get; set; }
    }
}
