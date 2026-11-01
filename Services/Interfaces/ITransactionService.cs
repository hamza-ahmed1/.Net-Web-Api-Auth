using Auth.Model.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<IActionResult> CreateTransaction(Guid invoiceId, TransactionDto transactionDto, string userId, FeeScope scope);
        public Task<IActionResult> ReverseTransaction(Guid transactionId, string reason, string userId, FeeScope scope);
        public Task<IActionResult> GetTransactions(TransactionQuery filter, FeeScope scope);
        public Task<IActionResult> GetReceipt(Guid transactionId);
        public Task<Guid?> GetStudentIdAsync(Guid transactionId);
    }
}
