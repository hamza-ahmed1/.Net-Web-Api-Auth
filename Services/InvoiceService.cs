using Auth.Data;
using Auth.Model.DTOs;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;

namespace Auth.Services
{
    public class InvoiceService:IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> GetPendingFeeForStudent(Guid studentId)
        {
            var pendingFees = await _context.ApplicableFees
                .Include(f => f.FeeType)    
                .Where(f => f.StudentId == studentId && !f.Status.Equals("Paid"))
                .ToListAsync();

            return new OkObjectResult(pendingFees);
        }
        public async Task<IActionResult> GetInvoiceForStudent(Guid studentId)
        {
            var invoices = await _context.Invoices
    .Where(i => i.StudentId == studentId)
    .Include(i => i.Student)
    .Include(i => i.feeType)
    .Select(i => new InvoiceDetailsDto
    {
        Id = i.Id,
        InvoiceNum = i.InvoiceNum,
        StudentId = i.StudentId,
        StudentName = i.Student.User.FullName, 
        FeeTypeId = i.FeeTypeId,
        FeeTypeName = i.FeeTypeName, 
        Amount = i.Amount,
        AmountPaid = i.AmountPaid,
        Currency = i.Currency,
        Month = i.Month,
        Year = i.Year,
        DueDate = i.DueDate,
        CreatedAt = i.CreatedAt
    })
    .ToListAsync();
            return new OkObjectResult(invoices);
        }
        public async Task<IActionResult> GenerateInvoice(List<ApplicableFee> fee, string Month, string Year, DateTime DueDate, Guid User)
        {
            if (fee == null || !fee.Any())
            {
                return new BadRequestObjectResult("No applicable fees provided.");
            }

            var studentId = fee.First().StudentId;
            if (fee.Any(f => f.StudentId != studentId))
            {
                return new BadRequestObjectResult("All fee entries must belong to the same student.");
            }

            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return new NotFoundObjectResult("Student not found.");
            }

            var feeTypeIds = fee
                .Select(item => item.FeeTypeId)
                .Distinct()
                .ToList();

            var feeTypes = await _context.FeeTypes
                .Where(item => feeTypeIds.Contains(item.FeeTypeId))
                .ToDictionaryAsync(item => item.FeeTypeId);

            // Guard: make sure every requested FeeTypeId actually exists before building invoices
            var missingFeeTypeIds = feeTypeIds.Where(id => !feeTypes.ContainsKey(id)).ToList();
            if (missingFeeTypeIds.Any())
            {
                return new BadRequestObjectResult(
                    $"Invalid fee type id(s): {string.Join(", ", missingFeeTypeIds)}");
            }

            string invoiceNum = await GenerateInvoiceNumberAsync();

            var invoices = fee.Select(applicableFee =>
            {
                var feeType = feeTypes[applicableFee.FeeTypeId];
                return new Invoice
                {
                    InvoiceNum = invoiceNum,
                    StudentId = studentId,
                    AmountPaid = 0,
                    Amount = feeType.Amount,
                    Currency = "PKR",
                    DueDate = DueDate,
                    UserId = User,
                    FeeTypeId = applicableFee.FeeTypeId,
                    FeeTypeName = feeType.Name,
                    Month = Month,
                    Year = Year,
                    CreatedAt = DateTime.UtcNow
                };
            }).ToList();

            _context.Invoices.AddRange(invoices);
            await _context.SaveChangesAsync();

            return new OkObjectResult(invoices);
        }
        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var today = DateTime.UtcNow;
            var count = await _context.Invoices.CountAsync(i => i.CreatedAt.Date == today.Date);
            return $"INV-{today:yyyyMMdd}-{(count + 1):D4}";
        }
    }
}
