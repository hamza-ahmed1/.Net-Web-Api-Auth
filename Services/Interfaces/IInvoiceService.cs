using Auth.Model.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IInvoiceService
    {
        public Task<IActionResult> GenerateInvoice(List<ApplicableFee> fee, string Month, string Year, DateTime DueDate, Guid User);
        public Task<IActionResult> GetPendingFeeForStudent(Guid studentId);
        public Task<IActionResult> GetInvoiceForStudent(Guid studentId);

    }
}
