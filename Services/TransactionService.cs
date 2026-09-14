using Auth.Data;
using Auth.Model.DTOs;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class TransactionService:ITransactionService
    {
        private readonly ApplicationDbContext _context;
        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> CreateTransaction(TransactionDto transactionDto)
        {
            // Check if invoice exists
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == transactionDto.invoiceID);

            if (invoice == null)
            {
                return new BadRequestObjectResult("Invoice not found.");
            }

            // Update AmountPaid
            invoice.AmountPaid += transactionDto.Amount;

            // Create transaction history
            var trx = new TransactionHistory
            {
                InvoiceId = transactionDto.invoiceID,
                Amount = transactionDto.Amount,
                Mode = transactionDto.Mode,
                TransactionReference = transactionDto.TransactionRef
            };

            _context.TransactionHistories.Add(trx);

            // Save both changes together
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Transaction created successfully",
                isSucceed = true
            });
        }

        public async Task<IActionResult> UpdateTransatoinStatusToUnpaid(Guid invoiceID)
        {
            // Check if invoice exists
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceID);

            if (invoice == null)
            {
                return new BadRequestObjectResult("Invoice not found.");
            }
            var trx = new TransactionHistory
            {
                InvoiceId = invoiceID,
                Amount = 0,
                Mode = 0,
                TransactionReference = "Amount set to 0"
            };
            // Update AmountPaid
            invoice.AmountPaid = 0;
            _context.TransactionHistories.Add(trx);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Transaction updated successfully",
                isSucceed = true
            });
        }
        public async Task<IActionResult> GetAllTransactions()
        {
             var transactions = await _context.TransactionHistories
                .Include(t => t.Invoice)
                .ToListAsync();
            return new OkObjectResult(transactions);
        }
    }
}
