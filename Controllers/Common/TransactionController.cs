using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("pay/{invoiceId}")]
        public async Task<IActionResult> CreateTransaction([FromBody] Model.DTOs.TransactionDto transactionDto)
        {
            var result = await _transactionService.CreateTransaction(transactionDto);
            return result;
        }

        [HttpPost("mark-unpaid/{invoiceId}")]
        public async Task<IActionResult> UpdateTransactionStatusToUnpaid(Guid invoiceId)
        {
            var result = await _transactionService.UpdateTransatoinStatusToUnpaid(invoiceId);
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTransactions()
        {
            var result = await _transactionService.GetAllTransactions();
            return result;
        }
    }
}
