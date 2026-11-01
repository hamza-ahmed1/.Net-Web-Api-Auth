using Auth.Model.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IInvoiceService
    {
        public Task<IActionResult> GenerateInvoice(GenerateInvoiceRequest request, Guid user, FeeScope scope);

        public Task<IActionResult> GenerateBulkInvoice(GenerateInvoiceRequest request, Guid user, FeeScope scope);

        public Task<IActionResult> GetInvoiceForStudent(Guid studentId, bool includeVoided);
        public Task<IActionResult> ListInvoices(FeeScope scope, string? status, string? month, string? year, Guid? sectionId, string? search, int page, int pageSize);
        public Task<IActionResult> VoidInvoice(Guid invoiceId, string reason, string userId, FeeScope scope);
        public Task<IActionResult> GetAdjustments(Guid invoiceId, FeeScope scope);
        public Task<IActionResult> AddAdjustment(Guid invoiceId, AddAdjustmentDto dto, string userId, FeeScope scope);
        public Task<IActionResult> RemoveAdjustment(Guid invoiceId, Guid adjustmentId, string reason, string userId, FeeScope scope);
    }
}
