using System.Globalization;
using Auth.Data;
using Auth.Model.DTOs;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    /// <summary>Read-only fee reports. Cancelled invoices are left out of every total.</summary>
    public class FeeReportService : IFeeReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly FeeAccessService _access;

        public FeeReportService(ApplicationDbContext context, FeeAccessService access)
        {
            _context = context;
            _access = access;
        }

        public async Task<FeeSummaryDto> GetSummaryAsync(FeeReportQuery filter, FeeScope scope)
        {
            var query = _access.Invoices(scope).AsNoTracking().Where(i => !i.IsVoided);
            if (!string.IsNullOrWhiteSpace(filter.Month))
                query = query.Where(i => i.Month == filter.Month);
            if (!string.IsNullOrWhiteSpace(filter.Year))
                query = query.Where(i => i.Year == filter.Year);
            if (filter.SectionId != null)
                query = query.Where(i => i.Student!.StudentEnrollments.Any(e => e.IsActive && e.SectionId == filter.SectionId));

            var currencies = await CurrenciesAsync(query);
            var currency = Choose(filter.Currency, currencies);
            query = query.Where(i => i.Currency == currency);

            var invoices = (await query
                .Select(i => new { i.Amount, i.Discount, i.LateFee, i.AmountPaid, i.DueDate, i.FeeTypeName, i.Currency })
                .ToListAsync())
                .Select(i => new
                {
                    i.Amount,
                    i.Discount,
                    i.LateFee,
                    i.AmountPaid,
                    i.DueDate,
                    i.FeeTypeName,
                    i.Currency,
                    Net = i.Amount + i.LateFee - i.Discount,
                    Due = Math.Max(i.Amount + i.LateFee - i.Discount - i.AmountPaid, 0),
                    Status = FeeRules.InvoiceStatus(i.Amount + i.LateFee - i.Discount, i.AmountPaid, i.DueDate, false)
                })
                .ToList();

            var modes = await _context.TransactionHistories.AsNoTracking()
                .Where(t => query.Any(i => i.Id == t.InvoiceId))
                .GroupBy(t => t.Mode)
                .Select(g => new { Mode = g.Key, Amount = g.Sum(t => t.Amount) })
                .ToListAsync();

            return new FeeSummaryDto
            {
                Currency = currency,
                Currencies = currencies,
                Invoices = invoices.Count,
                Billed = invoices.Sum(i => i.Amount),
                Discounts = invoices.Sum(i => i.Discount),
                LateFees = invoices.Sum(i => i.LateFee),
                NetBilled = invoices.Sum(i => i.Net),
                Collected = invoices.Sum(i => i.AmountPaid),
                Outstanding = invoices.Sum(i => i.Due),
                Overdue = invoices.Where(i => i.Status == FeeRules.Overdue).Sum(i => i.Due),
                ByStatus = invoices.GroupBy(i => i.Status).ToDictionary(g => g.Key, g => g.Count()),
                ByMonth = invoices
                    .GroupBy(i => new DateTime(i.DueDate.Year, i.DueDate.Month, 1))
                    .OrderBy(g => g.Key)
                    .Select(g => new FeeReportRow(g.Key.ToString("MMM yyyy", CultureInfo.InvariantCulture), g.Count(), g.Sum(i => i.Net), g.Sum(i => i.AmountPaid), g.Sum(i => i.Due)))
                    .ToList(),
                ByFeeType = invoices
                    .GroupBy(i => i.FeeTypeName)
                    .OrderBy(g => g.Key)
                    .Select(g => new FeeReportRow(g.Key, g.Count(), g.Sum(i => i.Net), g.Sum(i => i.AmountPaid), g.Sum(i => i.Due)))
                    .ToList(),
                ByMode = modes.OrderByDescending(m => m.Amount).Select(m => new PaymentModeRow(m.Mode.ToString(), m.Amount)).ToList()
            };
        }

        public async Task<FeePage<DefaulterDto>> GetDefaultersAsync(Guid? sectionId, string? currency, FeeScope scope, int page, int pageSize)
        {
            var today = FeeRules.Today;
            var query = _access.Invoices(scope).AsNoTracking()
                .Where(i => !i.IsVoided && i.DueDate < today && i.Amount + i.LateFee - i.Discount - i.AmountPaid > 0);
            if (sectionId != null)
                query = query.Where(i => i.Student!.StudentEnrollments.Any(e => e.IsActive && e.SectionId == sectionId));

            var chosen = Choose(currency, await CurrenciesAsync(query));
            query = query.Where(i => i.Currency == chosen);

            var grouped = query.GroupBy(i => i.StudentId).Select(g => new
            {
                StudentId = g.Key,
                Invoices = g.Count(),
                Overdue = g.Sum(i => i.Amount + i.LateFee - i.Discount - i.AmountPaid),
                OldestDueDate = g.Min(i => i.DueDate)
            });

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var rows = await grouped.OrderByDescending(g => g.Overdue).ThenBy(g => g.StudentId).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var ids = rows.Select(r => r.StudentId).ToList();
            var students = await StudentListQuery(_context.Students.Where(s => ids.Contains(s.StudentId))).ToDictionaryAsync(s => s.StudentId);

            return new FeePage<DefaulterDto>
            {
                Total = await grouped.CountAsync(),
                Page = page,
                PageSize = pageSize,
                Items = rows.Select(r => new DefaulterDto
                {
                    StudentId = r.StudentId,
                    StudentName = students[r.StudentId].FullName,
                    StudentCode = students[r.StudentId].StudentCode,
                    Section = students[r.StudentId].Section,
                    Invoices = r.Invoices,
                    Overdue = r.Overdue,
                    OldestDueDate = r.OldestDueDate
                }).ToList()
            };
        }

        public async Task<LedgerDto?> GetLedgerAsync(Guid studentId, string? currency)
        {
            var student = await StudentListQuery(_context.Students.Where(s => s.StudentId == studentId)).FirstOrDefaultAsync();
            if (student == null)
                return null;

            var studentInvoices = _context.Invoices.AsNoTracking().Where(i => i.StudentId == studentId);
            var currencies = await CurrenciesAsync(studentInvoices);
            var chosen = Choose(currency, currencies);
            var invoices = await studentInvoices.Include(i => i.Adjustments).Where(i => i.Currency == chosen).ToListAsync();
            var invoiceIds = invoices.Select(i => i.Id).ToList();
            var transactions = await _context.TransactionHistories.AsNoTracking().Where(t => invoiceIds.Contains(t.InvoiceId)).ToListAsync();

            // (when, type, what, invoice, change in the amount owed)
            var moves = new List<(DateTime Date, string Type, string Description, string InvoiceNum, decimal Change)>();
            foreach (var invoice in invoices)
            {
                moves.Add((invoice.CreatedAt, "Invoice", $"{invoice.FeeTypeName} - {invoice.Month} {invoice.Year}", invoice.InvoiceNum, invoice.Amount));

                foreach (var adjustment in invoice.Adjustments)
                {
                    var change = adjustment.Type == AdjustmentType.LateFee ? adjustment.Amount : -adjustment.Amount;
                    moves.Add((adjustment.CreatedAt, adjustment.Type.ToString(), adjustment.Reason, invoice.InvoiceNum, change));
                    if (adjustment.IsRemoved)
                        moves.Add((adjustment.RemovedAt ?? adjustment.CreatedAt, $"{adjustment.Type} removed", adjustment.RemoveReason ?? string.Empty, invoice.InvoiceNum, -change));
                }

                if (invoice.IsVoided)
                    moves.Add((invoice.VoidedAt ?? invoice.CreatedAt, "Cancelled", invoice.VoidReason ?? string.Empty, invoice.InvoiceNum, -invoice.NetAmount));
            }
            foreach (var transaction in transactions)
            {
                var number = invoices.First(i => i.Id == transaction.InvoiceId).InvoiceNum;
                var description = transaction.Kind == TransactionKind.Reversal ? transaction.Reason ?? string.Empty : $"{transaction.Mode} ({transaction.TransactionReference})";
                moves.Add((transaction.PaidAt, transaction.Kind.ToString(), description, number, -transaction.Amount));
            }

            var balance = 0m;
            var entries = new List<LedgerEntry>();
            foreach (var move in moves.OrderBy(m => m.Date))
            {
                balance += move.Change;
                entries.Add(new LedgerEntry(move.Date, move.Type, move.Description, move.InvoiceNum, Math.Max(move.Change, 0), Math.Max(-move.Change, 0), balance));
            }

            return new LedgerDto
            {
                StudentId = studentId,
                StudentName = student.FullName,
                StudentCode = student.StudentCode,
                Section = student.Section,
                Currency = chosen,
                Currencies = currencies,
                Balance = balance,
                Entries = entries
            };
        }

        public Task<List<FeeStudentDto>> GetStudentsAsync(FeeScope scope) =>
            StudentListQuery(_access.Students(scope)).OrderBy(s => s.FullName).ToListAsync();

        // Different currencies are never added together, so each total covers one, the most used first
        private static Task<List<string>> CurrenciesAsync(IQueryable<Invoice> invoices) =>
            invoices.GroupBy(i => i.Currency).OrderByDescending(g => g.Count()).Select(g => g.Key).ToListAsync();

        private static string Choose(string? requested, List<string> available) =>
            !string.IsNullOrWhiteSpace(requested) ? requested.Trim().ToUpperInvariant() : available.FirstOrDefault() ?? "PKR";

        private static IQueryable<FeeStudentDto> StudentListQuery(IQueryable<Student> students) =>
            students.AsNoTracking().Select(s => new FeeStudentDto
            {
                StudentId = s.StudentId,
                FullName = s.User.FullName,
                StudentCode = s.StudentCode,
                Section = s.StudentEnrollments
                    .Where(e => e.IsActive)
                    .Select(e => e.EnrolledSection!.IntermediateClass + " - " + e.EnrolledSection.SectionName)
                    .FirstOrDefault(),
                SectionId = s.StudentEnrollments.Where(e => e.IsActive).Select(e => (Guid?)e.SectionId).FirstOrDefault()
            });
    }
}
