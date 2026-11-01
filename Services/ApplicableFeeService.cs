using System.Linq.Expressions;
using Auth.Data;
using Auth.Model.DTOs;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class ApplicableFeeService : IApplicableFeeService
    {
        private readonly ApplicationDbContext _context;
        private readonly FeeAccessService _access;

        public ApplicableFeeService(ApplicationDbContext context, FeeAccessService access)
        {
            _context = context;
            _access = access;
        }

        private static readonly Expression<Func<ApplicableFee, ApplicableFeeWithDetailsDto>> ToDetails = af => new ApplicableFeeWithDetailsDto
        {
            AfId = af.AfId,
            StudentId = af.StudentId,
            StudentName = af.Student!.User.FullName,
            FeeTypeId = af.FeeTypeId,
            FeeTypeName = af.FeeType!.Name,
            Amount = af.FeeType.Amount,
            Currency = af.FeeType.Currency,
            Per = af.FeeType.Per,
            AcademicTerm = af.FeeType.AcademicTerm,
            CreatedAt = af.CreatedAt
        };

        private IQueryable<ApplicableFee> InScope(FeeScope scope)
        {
            var students = _access.Students(scope);
            return _context.ApplicableFees.AsNoTracking().Where(af => students.Any(s => s.StudentId == af.StudentId));
        }

        public async Task<IActionResult> CreateApplicableFee(ApplicableFeeDto applicableFeeDto, FeeScope scope)
        {
            var problem = await FindProblemAsync(applicableFeeDto, scope);
            if (problem != null)
                return new BadRequestObjectResult(new { message = problem });

            _context.ApplicableFees.Add(new ApplicableFee { StudentId = applicableFeeDto.StudentId, FeeTypeId = applicableFeeDto.FeeTypeId });
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Applicable fee created successfully", IsSucceed = true });
        }

        public async Task<IActionResult> ApplyFeeBulk(List<ApplicableFeeDto> applicableFeeDtos, FeeScope scope)
        {
            var created = 0;
            var skipped = new List<SkippedFee>();
            foreach (var dto in applicableFeeDtos.DistinctBy(d => (d.StudentId, d.FeeTypeId)))
            {
                var problem = await FindProblemAsync(dto, scope);
                if (problem != null)
                {
                    skipped.Add(new SkippedFee(dto.StudentId, string.Empty, string.Empty, problem));
                    continue;
                }
                _context.ApplicableFees.Add(new ApplicableFee { StudentId = dto.StudentId, FeeTypeId = dto.FeeTypeId });
                created++;
            }

            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Applicable fees applied successfully", IsSucceed = true, created, skipped });
        }

        private async Task<string?> FindProblemAsync(ApplicableFeeDto dto, FeeScope scope, Guid? ignoring = null)
        {
            var student = await _access.Students(scope).AsNoTracking()
                .Where(s => s.StudentId == dto.StudentId)
                .Select(s => new { s.IsActive })
                .FirstOrDefaultAsync();
            if (student == null) return "Student not found in your department.";
            if (!student.IsActive) return "Student is inactive.";

            var feeType = await _context.FeeTypes.AsNoTracking().FirstOrDefaultAsync(f => f.FeeTypeId == dto.FeeTypeId);
            if (feeType == null) return "Fee type not found.";
            if (!feeType.IsActive) return "Fee type is inactive.";

            var duplicate = await _context.ApplicableFees.AnyAsync(af =>
                af.StudentId == dto.StudentId && af.FeeTypeId == dto.FeeTypeId && af.AfId != ignoring);
            return duplicate ? "This fee is already applied to the student." : null;
        }

        public async Task<IActionResult> GetAllApplicableFees(FeeScope scope)
        {
            var applicableFees = await InScope(scope).Select(ToDetails).ToListAsync();
            await SetStatusAsync(applicableFees);
            return new OkObjectResult(applicableFees);
        }

        public async Task<IActionResult> GetApplicableFeesById(Guid id, FeeScope scope)
        {
            var applicableFees = await InScope(scope).Where(af => af.AfId == id).Select(ToDetails).ToListAsync();
            if (applicableFees.Count == 0)
                return new NotFoundObjectResult(new { message = "Applicable fee not found" });

            await SetStatusAsync(applicableFees);
            return new OkObjectResult(applicableFees[0]);
        }

        public async Task<IActionResult> GetApplicableFeesByStudentId(Guid studentId, FeeScope scope)
        {
            var applicableFees = await InScope(scope).Where(af => af.StudentId == studentId).Select(ToDetails).ToListAsync();
            await SetStatusAsync(applicableFees);
            return new OkObjectResult(applicableFees);
        }

        /// <summary>Fees that can still go on a new invoice. Monthly fees stay listed after being paid.</summary>
        public async Task<IActionResult> GetBillableFeesForStudent(Guid studentId, FeeScope scope)
        {
            var billedOnce = _context.Invoices.Where(i => i.StudentId == studentId && !i.IsVoided);
            var applicableFees = await InScope(scope)
                .Where(af => af.StudentId == studentId && af.FeeType!.IsActive)
                .Where(af => af.FeeType!.Per != "one-time" || !billedOnce.Any(i => i.FeeTypeId == af.FeeTypeId))
                .Select(ToDetails)
                .ToListAsync();
            await SetStatusAsync(applicableFees);
            return new OkObjectResult(applicableFees);
        }

        public async Task<IActionResult> UpdateApplicableFee(Guid id, ApplicableFeeDto applicableFeeDto, FeeScope scope)
        {
            var existing = await InScope(scope).AsTracking().FirstOrDefaultAsync(af => af.AfId == id);
            if (existing == null)
                return new NotFoundObjectResult(new { message = "Applicable fee not found" });
            if (await IsBilledAsync(existing))
                return new BadRequestObjectResult(new { message = "This fee has invoices. Cancel them before changing it." });

            var problem = await FindProblemAsync(applicableFeeDto, scope, id);
            if (problem != null)
                return new BadRequestObjectResult(new { message = problem });

            existing.StudentId = applicableFeeDto.StudentId;
            existing.FeeTypeId = applicableFeeDto.FeeTypeId;
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Applicable fee updated successfully", IsSucceed = true });
        }

        public async Task<IActionResult> DeleteApplicableFee(Guid id, FeeScope scope)
        {
            var existing = await InScope(scope).AsTracking().FirstOrDefaultAsync(af => af.AfId == id);
            if (existing == null)
                return new NotFoundObjectResult(new { message = "Applicable fee not found" });
            if (await IsBilledAsync(existing))
                return new BadRequestObjectResult(new { message = "This fee has invoices. Cancel them before removing it." });

            _context.ApplicableFees.Remove(existing);
            await _context.SaveChangesAsync();
            return new OkObjectResult(new { message = "Applicable fee removed", IsSucceed = true });
        }

        // by section id
        public async Task<IActionResult> GetbulkApplicableFeeBySectionId(Guid sectionId, FeeScope scope)
        {
            var applicableFees = await InScope(scope)
                .Where(af => af.Student!.StudentEnrollments.Any(se => se.SectionId == sectionId && se.IsActive))
                .Select(ToDetails)
                .ToListAsync();
            await SetStatusAsync(applicableFees);
            return new OkObjectResult(applicableFees);
        }

        private Task<bool> IsBilledAsync(ApplicableFee fee) =>
            _context.Invoices.AnyAsync(i => i.StudentId == fee.StudentId && i.FeeTypeId == fee.FeeTypeId && !i.IsVoided);

        private async Task SetStatusAsync(List<ApplicableFeeWithDetailsDto> fees)
        {
            var studentIds = fees.Select(f => f.StudentId).Distinct().ToList();
            var invoices = await _context.Invoices.AsNoTracking()
                .Where(i => !i.IsVoided && studentIds.Contains(i.StudentId))
                .Select(i => new { i.StudentId, i.FeeTypeId, Net = i.Amount + i.LateFee - i.Discount, i.AmountPaid, i.DueDate })
                .ToListAsync();
            var statuses = invoices.ToLookup(
                i => (i.StudentId, i.FeeTypeId),
                i => FeeRules.InvoiceStatus(i.Net, i.AmountPaid, i.DueDate, false));

            foreach (var fee in fees)
                fee.Status = FeeRules.ApplicableStatus(statuses[(fee.StudentId, fee.FeeTypeId)].ToList()).ToString();
        }
    }
}
