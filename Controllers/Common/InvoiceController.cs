using System.Security.Claims;
using Auth.Model.DTOs;
using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = FeeRules.Staff)]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IApplicableFeeService _applicableFees;
        private readonly FeeAccessService _access;

        public InvoiceController(IInvoiceService invoiceService, IApplicableFeeService applicableFees, FeeAccessService access)
        {
            _invoiceService = invoiceService;
            _applicableFees = applicableFees;
            _access = access;
        }

        private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        [HttpGet]
        public async Task<IActionResult> ListInvoices(
            [FromQuery] string? status,
            [FromQuery] string? month,
            [FromQuery] string? year,
            [FromQuery] Guid? sectionId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25) =>
            await _invoiceService.ListInvoices(await _access.GetScopeAsync(User), status, month, year, sectionId, search, page, pageSize);

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceRequest request)
        {
            if (request.Fees.Count == 0)
                return BadRequest(new { message = "No applicable fees provided." });
            if (!Guid.TryParse(UserId, out var userId))
                return Unauthorized(new { message = "Invalid or missing user identity." });

            return await _invoiceService.GenerateInvoice(request, userId, await _access.GetScopeAsync(User));
        }

        [HttpPost("generate/bulk")]
        public async Task<IActionResult> GenerateBulkInvoice([FromBody] GenerateInvoiceRequest request)
        {
            if (request.Fees.Count == 0)
                return BadRequest(new { message = "No applicable fees provided." });
            if (!Guid.TryParse(UserId, out var userId))
                return Unauthorized(new { message = "Invalid or missing user identity." });

            return await _invoiceService.GenerateBulkInvoice(request, userId, await _access.GetScopeAsync(User));
        }

        [HttpPost("{id}/void")]
        public async Task<IActionResult> VoidInvoice(Guid id, [FromBody] ReasonDto dto) =>
            await _invoiceService.VoidInvoice(id, dto.Reason, UserId, await _access.GetScopeAsync(User));

        [HttpGet("{id}/adjustments")]
        public async Task<IActionResult> GetAdjustments(Guid id) =>
            await _invoiceService.GetAdjustments(id, await _access.GetScopeAsync(User));

        [HttpPost("{id}/adjustments")]
        public async Task<IActionResult> AddAdjustment(Guid id, [FromBody] AddAdjustmentDto dto) =>
            await _invoiceService.AddAdjustment(id, dto, UserId, await _access.GetScopeAsync(User));

        [HttpPost("{id}/adjustments/{adjustmentId}/remove")]
        public async Task<IActionResult> RemoveAdjustment(Guid id, Guid adjustmentId, [FromBody] ReasonDto dto) =>
            await _invoiceService.RemoveAdjustment(id, adjustmentId, dto.Reason, UserId, await _access.GetScopeAsync(User));

        /// <summary>The student's fees that can go on a new invoice.</summary>
        [HttpGet("pending/{studentId}")]
        public async Task<IActionResult> GetPendingFees(Guid studentId) =>
            await _applicableFees.GetBillableFeesForStudent(studentId, await _access.GetScopeAsync(User));
    }
}
