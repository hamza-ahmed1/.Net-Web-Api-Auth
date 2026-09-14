using Auth.Model.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<IActionResult> CreateTransaction(TransactionDto transactionDto);
        public Task<IActionResult> UpdateTransatoinStatusToUnpaid(Guid invoiceID);

        public Task<IActionResult> GetAllTransactions();
    }
}
