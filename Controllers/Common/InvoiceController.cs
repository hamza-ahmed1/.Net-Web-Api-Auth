using Auth.Model.DTOs;
using Auth.Model.Entities;
using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.Controllers.Common
{

    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInvoiceService _invoiceService;
        public InvoiceController(
            IInvoiceService invoiceService,
            UserManager<ApplicationUser> userManager)
        {
            _invoiceService = invoiceService;
            _userManager = userManager;
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
            var result = await _invoiceService.GetPendingFeeForStudent(studentId);
            return result;
        }

        [HttpGet("history/{studentId}")]
        public async Task<IActionResult> GetInvoiceHistory(Guid studentId)
        {
            var result = await _invoiceService.GetInvoiceForStudent(studentId);
            return result;
        }
    }
}
