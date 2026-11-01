using Auth.Model.Entities;

namespace Auth.Services
{
    /// <summary>The fee rules in one place, so every screen and report agrees.</summary>
    public static class FeeRules
    {
        public const string Staff = "Admin,HOD";

        public const string Unpaid = "Unpaid";
        public const string Partial = "Partial";
        public const string Paid = "Paid";
        public const string Overdue = "Overdue";
        public const string Voided = "Voided";

        private static readonly string[] Months =
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        public static DateTime Today => DateTime.UtcNow.Date;

        /// <summary>Returns the full month name, or null when the text is not a month.</summary>
        public static string? NormalizeMonth(string? month) =>
            Months.FirstOrDefault(m => string.Equals(m, month?.Trim(), StringComparison.OrdinalIgnoreCase));

        public static bool IsValidYear(string? year) =>
            int.TryParse(year?.Trim(), out var value) && value is >= 2000 and <= 2100;

        public static bool HasValidPrecision(decimal amount) => decimal.Round(amount, 2) == amount;

        public static string InvoiceStatus(decimal netAmount, decimal amountPaid, DateTime dueDate, bool isVoided)
        {
            if (isVoided) return Voided;
            if (netAmount - amountPaid <= 0) return Paid;
            if (dueDate.Date < Today) return Overdue;
            return amountPaid > 0 ? Partial : Unpaid;
        }

        /// <summary>Clear once every invoice is settled; a fee that has never been billed stays Pending.</summary>
        public static FeeStatus ApplicableStatus(IReadOnlyCollection<string> invoiceStatuses)
        {
            if (invoiceStatuses.Contains(Overdue)) return FeeStatus.Overdue;
            return invoiceStatuses.Count > 0 && invoiceStatuses.All(s => s == Paid) ? FeeStatus.Clear : FeeStatus.Pending;
        }

        /// <summary>Why this fee cannot be billed for the period, or null when it can.</summary>
        public static string? DuplicateBillingReason(string per, string month, string year, IEnumerable<(string Month, string Year)> existing)
        {
            var bills = existing.ToList();
            switch (per)
            {
                case "one-time":
                    return bills.Count > 0 ? "This one-time fee has already been billed." : null;
                case "year":
                    return bills.Any(b => b.Year == year) ? $"This yearly fee has already been billed for {year}." : null;
                default:
                    return bills.Any(b => b.Month == month && b.Year == year) ? $"Already billed for {month} {year}." : null;
            }
        }
    }
}
