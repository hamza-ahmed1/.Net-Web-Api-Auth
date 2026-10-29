using Auth.Model.DTOs;
using Auth.Model.Entities;
using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.Controllers.Common
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInvoiceService _invoiceService;
        private readonly TeachingAccessService _access;
        public InvoiceController(
            IInvoiceService invoiceService,
            UserManager<ApplicationUser> userManager,
            TeachingAccessService access)
        {
            _invoiceService = invoiceService;
            _userManager = userManager;
            _access = access;
        }


        [HttpPost("generate")]
        public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceRequest request)
        {
            if (request == null || request.Fees == null || !request.Fees.Any())
            {
                return BadRequest("No applicable fees provided.");
            }

            var userIdValue = _userManager.GetUserId(User);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("Invalid or missing user identity.");
            }

            var result = await _invoiceService.GenerateInvoice(
                request.Fees,
                request.Month,
                request.Year,
                request.DueDate,
                userId
            );

            return result;
        }

        [HttpPost("generate/bulk")] 

        public async Task<IActionResult> GenerateBulkInvoice([FromBody] GenerateInvoiceRequest request)
        {
            if (request == null || request.Fees == null || !request.Fees.Any())
            {
                return BadRequest("No applicable fees provided.");
            }
            var userIdValue = _userManager.GetUserId(User);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("Invalid or missing user identity.");
            }
            var result = await _invoiceService.GenerateBulkInvoice(
                request.Fees,
                request.Month,
                request.Year,
                request.DueDate,
                userId
            );
            return result;
        }

        [HttpGet("pending/{studentId}")]
        public async Task<IActionResult> GetPendingFees(Guid studentId)
        {
            if (!await CanReadStudentAsync(studentId))
                return Forbid();

            var result = await _invoiceService.GetPendingFeeForStudent(studentId);
            return result;
        }

        /// <summary>A student's challans (month, due date, amounts). Students may only read their own.</summary>
        [HttpGet("history/{studentId}")]
        public async Task<IActionResult> GetInvoiceHistory(Guid studentId)
        {
            if (!await CanReadStudentAsync(studentId))
                return Forbid();

            var result = await _invoiceService.GetInvoiceForStudent(studentId);
            return result;
        }

        private async Task<bool> CanReadStudentAsync(Guid studentId)
        {
            if (User.IsInRole("Admin") || User.IsInRole("HOD") || User.IsInRole("Teacher"))
                return true;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            return await _access.StudentIdForUserAsync(userId) == studentId;
        }
    }
}
