using Auth.Model.DTOs;

namespace Auth.Services.Interfaces
{
    public interface IFeeReportService
    {
        Task<FeeSummaryDto> GetSummaryAsync(FeeReportQuery filter, FeeScope scope);
        Task<FeePage<DefaulterDto>> GetDefaultersAsync(Guid? sectionId, string? currency, FeeScope scope, int page, int pageSize);
        Task<LedgerDto?> GetLedgerAsync(Guid studentId, string? currency);
        Task<List<FeeStudentDto>> GetStudentsAsync(FeeScope scope);
    }
}
