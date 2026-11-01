using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{
    /// <summary>
    /// The fee records a student may read about themselves. These live apart from the staff controllers because
    /// [Authorize] rules add up: a student could never pass a staff-only rule on the controller.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Admin,HOD,Student")]
    public class StudentFeeController : ControllerBase
    {
        private readonly IInvoiceService _invoices;
        private readonly ITransactionService _transactions;
        private readonly IFeeReportService _reports;
        private readonly FeeAccessService _access;

        public StudentFeeController(IInvoiceService invoices, ITransactionService transactions, IFeeReportService reports, FeeAccessService access)
        {
            _invoices = invoices;
            _transactions = transactions;
            _reports = reports;
            _access = access;
        }

        /// <summary>A student's challans (month, due date, amounts). Students may only read their own.</summary>
        [HttpGet("api/Invoice/history/{studentId}")]
        public async Task<IActionResult> GetInvoiceHistory(Guid studentId)
        {
            if (!await _access.CanReadStudentAsync(User, studentId))
                return Forbid();

            return await _invoices.GetInvoiceForStudent(studentId, includeVoided: !User.IsInRole("Student"));
        }

        /// <summary>Every charge and payment for one student, with a running balance.</summary>
        [HttpGet("api/FeeReport/ledger/{studentId}")]
        public async Task<IActionResult> Ledger(Guid studentId, [FromQuery] string? currency)
        {
            if (!await _access.CanReadStudentAsync(User, studentId))
                return Forbid();

            var ledger = await _reports.GetLedgerAsync(studentId, currency);
            return ledger == null ? NotFound(new { message = "Student not found." }) : Ok(ledger);
        }

        [HttpGet("api/Transaction/{transactionId}/receipt")]
        public async Task<IActionResult> GetReceipt(Guid transactionId)
        {
            var studentId = await _transactions.GetStudentIdAsync(transactionId);
            if (studentId == null)
                return NotFound(new { message = "Receipt not found." });
            if (!await _access.CanReadStudentAsync(User, studentId.Value))
                return Forbid();

            return await _transactions.GetReceipt(transactionId);
        }
    }
}
