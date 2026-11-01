using System.Linq.Expressions;
using Auth.Data;
using Auth.Model.DTOs;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class InvoiceService:IInvoiceService
    {
        private const string ChangedByOthers = "This invoice was just changed by someone else. Refresh and try again.";

        private readonly ApplicationDbContext _context;
        private readonly FeeAccessService _access;

        public InvoiceService(ApplicationDbContext context, FeeAccessService access)
        {
            _context = context;
            _access = access;
        }

        private static readonly Expression<Func<Invoice, InvoiceDetailsDto>> ToDetails = i => new InvoiceDetailsDto
        {
            Id = i.Id,
            InvoiceNum = i.InvoiceNum,
            StudentId = i.StudentId,
            StudentName = i.Student!.User.FullName,
            StudentCode = i.Student.StudentCode,
            Section = i.Student.StudentEnrollments
                .Where(e => e.IsActive)
                .Select(e => e.EnrolledSection!.IntermediateClass + " - " + e.EnrolledSection.SectionName)
                .FirstOrDefault(),
            FeeTypeId = i.FeeTypeId,
            FeeTypeName = i.FeeTypeName,
            Amount = i.Amount,
            Discount = i.Discount,
            LateFee = i.LateFee,
            AmountPaid = i.AmountPaid,
            Currency = i.Currency,
            Month = i.Month,
            Year = i.Year,
            DueDate = i.DueDate,
            CreatedAt = i.CreatedAt,
            IsVoided = i.IsVoided,
            VoidReason = i.VoidReason
        };

        public async Task<IActionResult> GetInvoiceForStudent(Guid studentId, bool includeVoided)
        {
            var invoices = await _context.Invoices
                .Where(i => i.StudentId == studentId && (includeVoided || !i.IsVoided))
                .OrderByDescending(i => i.DueDate)
                .Select(ToDetails)
                .ToListAsync();
            return new OkObjectResult(invoices);
        }

        public async Task<IActionResult> ListInvoices(FeeScope scope, string? status, string? month, string? year, Guid? sectionId, string? search, int page, int pageSize)
        {
            var query = _access.Invoices(scope).AsNoTracking();
            var today = FeeRules.Today;

            query = status?.Trim().ToLowerInvariant() switch
            {
                "voided" => query.Where(i => i.IsVoided),
                "paid" => query.Where(i => !i.IsVoided && i.Amount + i.LateFee - i.Discount - i.AmountPaid <= 0),
                "overdue" => query.Where(i => !i.IsVoided && i.Amount + i.LateFee - i.Discount - i.AmountPaid > 0 && i.DueDate < today),
                "partial" => query.Where(i => !i.IsVoided && i.AmountPaid > 0 && i.Amount + i.LateFee - i.Discount - i.AmountPaid > 0 && i.DueDate >= today),
                "unpaid" => query.Where(i => !i.IsVoided && i.AmountPaid == 0 && i.Amount + i.LateFee - i.Discount - i.AmountPaid > 0 && i.DueDate >= today),
                _ => query
            };
            if (!string.IsNullOrWhiteSpace(month))
                query = query.Where(i => i.Month == month.Trim());
            if (!string.IsNullOrWhiteSpace(year))
                query = query.Where(i => i.Year == year.Trim());
            if (sectionId != null)
                query = query.Where(i => i.Student!.StudentEnrollments.Any(e => e.IsActive && e.SectionId == sectionId));
            if (!string.IsNullOrWhiteSpace(search))
            {
                var text = search.Trim();
                query = query.Where(i => i.InvoiceNum.Contains(text) || i.Student!.User.FullName.Contains(text) || i.Student.StudentCode.Contains(text));
            }

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = new FeePage<InvoiceDetailsDto>
            {
                Total = await query.CountAsync(),
                Page = page,
                PageSize = pageSize,
                Items = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .ThenByDescending(i => i.InvoiceNum)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ToDetails)
                    .ToListAsync()
            };
            return new OkObjectResult(result);
        }

        /// <summary>One student: nothing is saved unless every fee can be billed.</summary>
        public async Task<IActionResult> GenerateInvoice(GenerateInvoiceRequest request, Guid user, FeeScope scope)
        {
            if (request.Fees.Select(f => f.StudentId).Distinct().Count() > 1)
                return new BadRequestObjectResult(new { message = "All fee entries must belong to the same student." });

            return await GenerateAsync(request, user, scope, allOrNothing: true);
        }

        /// <summary>Many students: fees that cannot be billed are skipped and reported.</summary>
        public Task<IActionResult> GenerateBulkInvoice(GenerateInvoiceRequest request, Guid user, FeeScope scope) =>
            GenerateAsync(request, user, scope, allOrNothing: false);

        private async Task<IActionResult> GenerateAsync(GenerateInvoiceRequest request, Guid user, FeeScope scope, bool allOrNothing)
        {
            var month = FeeRules.NormalizeMonth(request.Month);
            if (month == null)
                return new BadRequestObjectResult(new { message = "Choose a valid month." });
            if (!FeeRules.IsValidYear(request.Year))
                return new BadRequestObjectResult(new { message = "Enter a valid year." });
            if (request.DueDate.Date < FeeRules.Today)
                return new BadRequestObjectResult(new { message = "The due date cannot be in the past." });

            var year = request.Year.Trim();
            var items = request.Fees.DistinctBy(f => (f.StudentId, f.FeeTypeId)).ToList();
            var studentIds = items.Select(f => f.StudentId).Distinct().ToList();
            var feeTypeIds = items.Select(f => f.FeeTypeId).Distinct().ToList();

            var students = await _access.Students(scope).AsNoTracking()
                .Where(s => studentIds.Contains(s.StudentId))
                .Select(s => new { s.StudentId, s.User.FullName, s.IsActive, Enrolled = s.StudentEnrollments.Any(e => e.IsActive) })
                .ToDictionaryAsync(s => s.StudentId);
            var feeTypes = await _context.FeeTypes.AsNoTracking()
                .Where(f => feeTypeIds.Contains(f.FeeTypeId))
                .ToDictionaryAsync(f => f.FeeTypeId);
            var applied = (await _context.ApplicableFees.AsNoTracking()
                .Where(a => studentIds.Contains(a.StudentId) && feeTypeIds.Contains(a.FeeTypeId))
                .Select(a => new { a.StudentId, a.FeeTypeId })
                .ToListAsync())
                .Select(a => (a.StudentId, a.FeeTypeId))
                .ToHashSet();
            var billed = (await _context.Invoices.AsNoTracking()
                .Where(i => !i.IsVoided && studentIds.Contains(i.StudentId) && feeTypeIds.Contains(i.FeeTypeId))
                .Select(i => new { i.StudentId, i.FeeTypeId, i.Month, i.Year })
                .ToListAsync())
                .ToLookup(i => (i.StudentId, i.FeeTypeId), i => (i.Month, i.Year));

            var invoices = new List<Invoice>();
            var skipped = new List<SkippedFee>();
            foreach (var item in items)
            {
                students.TryGetValue(item.StudentId, out var student);
                feeTypes.TryGetValue(item.FeeTypeId, out var feeType);

                string? reason = null;
                if (student == null) reason = "Student not found in your department.";
                else if (!student.IsActive || !student.Enrolled) reason = "Student is inactive or withdrawn.";
                else if (feeType == null) reason = "Fee type not found.";
                else if (!feeType.IsActive) reason = "Fee type is inactive.";
                else if (!applied.Contains((item.StudentId, item.FeeTypeId))) reason = "This fee has not been applied to the student.";
                else reason = FeeRules.DuplicateBillingReason(feeType.Per, month, year, billed[(item.StudentId, item.FeeTypeId)]);

                if (reason != null)
                {
                    skipped.Add(new SkippedFee(item.StudentId, student?.FullName ?? "Unknown student", feeType?.Name ?? "Unknown fee", reason));
                    continue;
                }

                invoices.Add(new Invoice
                {
                    InvoiceNum = await NextInvoiceNumberAsync(),
                    StudentId = item.StudentId,
                    Amount = feeType!.Amount,
                    Currency = feeType.Currency,
                    DueDate = request.DueDate,
                    UserId = user,
                    FeeTypeId = item.FeeTypeId,
                    FeeTypeName = feeType.Name,
                    Month = month,
                    Year = year,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (allOrNothing && skipped.Count > 0)
                return new BadRequestObjectResult(new { message = $"{skipped[0].Fee}: {skipped[0].Reason}", skipped });

            _context.Invoices.AddRange(invoices);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // The unique index caught a second person billing the same period at the same moment
                return new ConflictObjectResult(new { message = "Some of these fees were billed by someone else just now. Refresh and try again." });
            }

            var createdIds = invoices.Select(i => i.Id).ToList();
            var created = await _context.Invoices.AsNoTracking().Where(i => createdIds.Contains(i.Id)).Select(ToDetails).ToListAsync();
            return new OkObjectResult(new { created, skipped });
        }

        public async Task<IActionResult> VoidInvoice(Guid invoiceId, string reason, string userId, FeeScope scope)
        {
            var invoice = await _access.Invoices(scope).FirstOrDefaultAsync(i => i.Id == invoiceId);
            if (invoice == null)
                return new NotFoundObjectResult(new { message = "Invoice not found." });
            if (invoice.IsVoided)
                return new BadRequestObjectResult(new { message = "This invoice is already cancelled." });
            if (invoice.AmountPaid != 0)
                return new BadRequestObjectResult(new { message = "This invoice has payments. Reverse them before cancelling it." });

            invoice.IsVoided = true;
            invoice.VoidedAt = DateTime.UtcNow;
            invoice.VoidedByUserId = userId;
            invoice.VoidReason = reason.Trim();
            return await SaveAsync(new { message = "Invoice cancelled.", isSucceed = true });
        }

        public async Task<IActionResult> GetAdjustments(Guid invoiceId, FeeScope scope)
        {
            if (!await _access.Invoices(scope).AnyAsync(i => i.Id == invoiceId))
                return new NotFoundObjectResult(new { message = "Invoice not found." });

            var adjustments = await _context.Set<InvoiceAdjustment>().AsNoTracking()
                .Where(a => a.InvoiceId == invoiceId)
                .OrderBy(a => a.CreatedAt)
                .Select(a => new AdjustmentDetailsDto
                {
                    Id = a.Id,
                    Type = a.Type.ToString(),
                    Amount = a.Amount,
                    Reason = a.Reason,
                    CreatedAt = a.CreatedAt,
                    RecordedBy = _context.Users.Where(u => u.Id == a.RecordedByUserId).Select(u => u.FullName).FirstOrDefault(),
                    IsRemoved = a.IsRemoved,
                    RemoveReason = a.RemoveReason
                })
                .ToListAsync();
            return new OkObjectResult(adjustments);
        }

        public async Task<IActionResult> AddAdjustment(Guid invoiceId, AddAdjustmentDto dto, string userId, FeeScope scope)
        {
            var invoice = await _access.Invoices(scope).FirstOrDefaultAsync(i => i.Id == invoiceId);
            if (invoice == null)
                return new NotFoundObjectResult(new { message = "Invoice not found." });
            if (invoice.IsVoided)
                return new BadRequestObjectResult(new { message = "This invoice is cancelled." });
            if (!Enum.IsDefined(dto.Type))
                return new BadRequestObjectResult(new { message = "Choose a discount, scholarship or late fee." });
            if (!FeeRules.HasValidPrecision(dto.Amount))
                return new BadRequestObjectResult(new { message = "Amount can have at most 2 decimal places." });

            if (dto.Type == AdjustmentType.LateFee)
            {
                if (invoice.DueDate.Date >= FeeRules.Today)
                    return new BadRequestObjectResult(new { message = "A late fee can only be added after the due date." });
                invoice.LateFee += dto.Amount;
            }
            else
            {
                if (dto.Amount > invoice.AmountDue)
                    return new BadRequestObjectResult(new { message = $"The amount cannot be more than the balance due ({invoice.AmountDue:0.##})." });
                invoice.Discount += dto.Amount;
            }

            _context.Set<InvoiceAdjustment>().Add(new InvoiceAdjustment
            {
                InvoiceId = invoiceId,
                Type = dto.Type,
                Amount = dto.Amount,
                Reason = dto.Reason.Trim(),
                RecordedByUserId = userId
            });
            return await SaveAsync(new { message = $"{dto.Type} added.", isSucceed = true });
        }

        public async Task<IActionResult> RemoveAdjustment(Guid invoiceId, Guid adjustmentId, string reason, string userId, FeeScope scope)
        {
            var invoice = await _access.Invoices(scope).FirstOrDefaultAsync(i => i.Id == invoiceId);
            if (invoice == null)
                return new NotFoundObjectResult(new { message = "Invoice not found." });
            if (invoice.IsVoided)
                return new BadRequestObjectResult(new { message = "This invoice is cancelled." });

            var adjustment = await _context.Set<InvoiceAdjustment>().FirstOrDefaultAsync(a => a.Id == adjustmentId && a.InvoiceId == invoiceId);
            if (adjustment == null || adjustment.IsRemoved)
                return new NotFoundObjectResult(new { message = "Adjustment not found." });

            if (adjustment.Type == AdjustmentType.LateFee)
            {
                // Removing a late fee that has been paid would leave the student overpaid
                if (adjustment.Amount > invoice.AmountDue)
                    return new BadRequestObjectResult(new { message = "This late fee has been paid. Reverse the payment first." });
                invoice.LateFee -= adjustment.Amount;
            }
            else
            {
                invoice.Discount -= adjustment.Amount;
            }

            adjustment.IsRemoved = true;
            adjustment.RemovedAt = DateTime.UtcNow;
            adjustment.RemovedByUserId = userId;
            adjustment.RemoveReason = reason.Trim();
            return await SaveAsync(new { message = "Adjustment removed.", isSucceed = true });
        }

        private async Task<IActionResult> SaveAsync(object success)
        {
            try
            {
                await _context.SaveChangesAsync();
                return new OkObjectResult(success);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ConflictObjectResult(new { message = ChangedByOthers });
            }
        }

        // A database sequence hands out each number once, even when two people bill at the same moment
        private async Task<string> NextInvoiceNumberAsync()
        {
            var next = await _context.Database
                .SqlQueryRaw<long>("SELECT NEXT VALUE FOR dbo.InvoiceNumbers AS [Value]")
                .ToListAsync();
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{next[0]:D6}";
        }
    }
}
