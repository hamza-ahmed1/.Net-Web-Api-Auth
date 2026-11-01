using System.Linq.Expressions;
using Auth.Data;
using Auth.Model.DTOs;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class TransactionService:ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly FeeAccessService _access;

        public TransactionService(ApplicationDbContext context, FeeAccessService access)
        {
            _context = context;
            _access = access;
        }

        private Expression<Func<TransactionHistory, TransactionDetailsDto>> ToDetails => t => new TransactionDetailsDto
        {
            TransactionId = t.TransactionId,
            InvoiceId = t.InvoiceId,
            InvoiceNum = t.Invoice!.InvoiceNum,
            FeeTypeName = t.Invoice.FeeTypeName,
            StudentId = t.Invoice.StudentId,
            StudentName = t.Invoice.Student!.User.FullName,
            Amount = t.Amount,
            Currency = t.Invoice.Currency,
            Mode = t.Mode.ToString(),
            Kind = t.Kind.ToString(),
            TransactionReference = t.TransactionReference,
            PaidAt = t.PaidAt,
            Reason = t.Reason,
            RecordedBy = _context.Users.Where(u => u.Id == t.RecordedByUserId).Select(u => u.FullName).FirstOrDefault(),
            IsReversed = _context.TransactionHistories.Any(r => r.ReversesTransactionId == t.TransactionId)
        };

        public async Task<IActionResult> CreateTransaction(Guid invoiceId, TransactionDto transactionDto, string userId, FeeScope scope)
        {
            if (transactionDto.invoiceID != Guid.Empty && transactionDto.invoiceID != invoiceId)
                return new BadRequestObjectResult(new { message = "The invoice in the request does not match." });

            var invoice = await _access.Invoices(scope).FirstOrDefaultAsync(i => i.Id == invoiceId);
            if (invoice == null)
                return new NotFoundObjectResult(new { message = "Invoice not found." });
            if (invoice.IsVoided)
                return new BadRequestObjectResult(new { message = "This invoice is cancelled." });
            if (!Enum.IsDefined(transactionDto.Mode))
                return new BadRequestObjectResult(new { message = "Choose a valid payment mode." });
            if (!FeeRules.HasValidPrecision(transactionDto.Amount))
                return new BadRequestObjectResult(new { message = "Amount can have at most 2 decimal places." });

            var due = invoice.AmountDue;
            if (due <= 0)
                return new BadRequestObjectResult(new { message = "This invoice is already paid." });
            if (transactionDto.Amount > due)
                return new BadRequestObjectResult(new { message = $"The amount cannot be more than the balance due ({due:0.##})." });

            var reference = transactionDto.TransactionRef.Trim();
            if (await _context.TransactionHistories.AnyAsync(t => t.TransactionReference == reference))
                return new ConflictObjectResult(new { message = "A payment with this reference has already been recorded." });

            invoice.AmountPaid += transactionDto.Amount;
            var trx = new TransactionHistory
            {
                InvoiceId = invoiceId,
                Amount = transactionDto.Amount,
                Mode = transactionDto.Mode,
                TransactionReference = reference,
                RecordedByUserId = userId
            };
            _context.TransactionHistories.Add(trx);

            var failure = await SaveAsync();
            if (failure != null)
                return failure;

            return new OkObjectResult(new
            {
                message = "Transaction created successfully",
                isSucceed = true,
                transactionId = trx.TransactionId,
                amountDue = invoice.AmountDue
            });
        }

        /// <summary>Cancels a whole payment with a negative entry. The original stays in the history.</summary>
        public async Task<IActionResult> ReverseTransaction(Guid transactionId, string reason, string userId, FeeScope scope)
        {
            var original = await _context.TransactionHistories.AsNoTracking().FirstOrDefaultAsync(t => t.TransactionId == transactionId);
            var invoice = original == null ? null : await _access.Invoices(scope).FirstOrDefaultAsync(i => i.Id == original.InvoiceId);
            if (original == null || invoice == null)
                return new NotFoundObjectResult(new { message = "Transaction not found." });
            if (original.Kind != TransactionKind.Payment || original.Amount <= 0)
                return new BadRequestObjectResult(new { message = "Only a payment can be reversed." });
            if (await _context.TransactionHistories.AnyAsync(t => t.ReversesTransactionId == transactionId))
                return new BadRequestObjectResult(new { message = "This payment has already been reversed." });

            invoice.AmountPaid -= original.Amount;
            _context.TransactionHistories.Add(new TransactionHistory
            {
                InvoiceId = invoice.Id,
                Amount = -original.Amount,
                Mode = original.Mode,
                TransactionReference = $"REV-{transactionId:N}"[..16],
                Kind = TransactionKind.Reversal,
                ReversesTransactionId = transactionId,
                Reason = reason.Trim(),
                RecordedByUserId = userId
            });

            return await SaveAsync() ?? new OkObjectResult(new { message = "Payment reversed.", isSucceed = true });
        }

        public async Task<IActionResult> GetTransactions(TransactionQuery filter, FeeScope scope)
        {
            var invoices = _access.Invoices(scope);
            var query = _context.TransactionHistories.AsNoTracking().Where(t => invoices.Any(i => i.Id == t.InvoiceId));

            if (filter.From != null)
                query = query.Where(t => t.PaidAt >= filter.From.Value.Date);
            if (filter.To != null)
                query = query.Where(t => t.PaidAt < filter.To.Value.Date.AddDays(1));
            if (filter.Mode != null)
                query = query.Where(t => t.Mode == filter.Mode);
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var text = filter.Search.Trim();
                query = query.Where(t =>
                    t.TransactionReference.Contains(text) ||
                    t.Invoice!.InvoiceNum.Contains(text) ||
                    t.Invoice.Student!.User.FullName.Contains(text) ||
                    t.Invoice.Student.StudentCode.Contains(text));
            }

            var page = Math.Max(filter.Page, 1);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);
            return new OkObjectResult(new FeePage<TransactionDetailsDto>
            {
                Total = await query.CountAsync(),
                Page = page,
                PageSize = pageSize,
                Items = await query
                    .OrderByDescending(t => t.PaidAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ToDetails)
                    .ToListAsync()
            });
        }

        public async Task<IActionResult> GetReceipt(Guid transactionId)
        {
            var details = await _context.TransactionHistories.AsNoTracking()
                .Where(t => t.TransactionId == transactionId)
                .Select(ToDetails)
                .FirstOrDefaultAsync();
            if (details == null || details.Kind != nameof(TransactionKind.Payment))
                return new NotFoundObjectResult(new { message = "Receipt not found." });

            var invoice = await _context.Invoices.AsNoTracking()
                .Where(i => i.Id == details.InvoiceId)
                .Select(i => new
                {
                    i.Amount,
                    i.LateFee,
                    i.Discount,
                    i.AmountPaid,
                    i.Month,
                    i.Year,
                    i.Student!.StudentCode,
                    Section = i.Student.StudentEnrollments
                        .Where(e => e.IsActive)
                        .Select(e => e.EnrolledSection!.IntermediateClass + " - " + e.EnrolledSection.SectionName)
                        .FirstOrDefault()
                })
                .FirstAsync();

            var net = invoice.Amount + invoice.LateFee - invoice.Discount;
            return new OkObjectResult(new ReceiptDto
            {
                ReceiptNo = $"RCT-{details.PaidAt:yyyyMMdd}-{transactionId.ToString("N")[..8].ToUpperInvariant()}",
                Transaction = details,
                StudentCode = invoice.StudentCode,
                Section = invoice.Section,
                Period = $"{invoice.Month} {invoice.Year}",
                InvoiceNet = net,
                InvoicePaid = invoice.AmountPaid,
                BalanceDue = Math.Max(net - invoice.AmountPaid, 0)
            });
        }

        public async Task<Guid?> GetStudentIdAsync(Guid transactionId) =>
            await _context.TransactionHistories
                .Where(t => t.TransactionId == transactionId)
                .Select(t => (Guid?)t.Invoice!.StudentId)
                .FirstOrDefaultAsync();

        private async Task<IActionResult?> SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ConflictObjectResult(new { message = "This invoice was just changed by someone else. Refresh and try again." });
            }
            catch (DbUpdateException)
            {
                // The unique indexes stop a double-submitted payment or reversal
                return new ConflictObjectResult(new { message = "This has already been recorded. Refresh to see it." });
            }
        }
    }
}
