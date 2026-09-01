using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IInvoiceService
    {
        public Task<IActionResult> GenerateInvoice(Guid StudentId,DateTime validDate);
    }
}
