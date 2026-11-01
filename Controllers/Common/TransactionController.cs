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
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly FeeAccessService _access;

        public TransactionController(ITransactionService transactionService, FeeAccessService access)
        {
            _transactionService = transactionService;
            _access = access;
        }

        private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        [HttpPost("pay/{invoiceId}")]
        public async Task<IActionResult> CreateTransaction(Guid invoiceId, [FromBody] TransactionDto transactionDto) =>
            await _transactionService.CreateTransaction(invoiceId, transactionDto, UserId, await _access.GetScopeAsync(User));

        [HttpPost("{transactionId}/reverse")]
        public async Task<IActionResult> ReverseTransaction(Guid transactionId, [FromBody] ReasonDto dto) =>
            await _transactionService.ReverseTransaction(transactionId, dto.Reason, UserId, await _access.GetScopeAsync(User));

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionQuery filter) =>
            await _transactionService.GetTransactions(filter, await _access.GetScopeAsync(User));
    }
}
