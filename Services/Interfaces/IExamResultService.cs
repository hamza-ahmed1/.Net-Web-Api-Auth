using Auth.Model.DTOs.ExamResult;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IExamResultService
    {
        public Task<IActionResult> CreateExamResult(ExamResultCreateDto dto);

        public Task<IActionResult> UploadBulkResult(List<ExamResultCreateDto> dtos);

        public Task<IActionResult> UpdateBulkResult(List<ExamResultCreateDto> dtos);
        public Task<IActionResult> UpdateExamResult(Guid examResultId, ExamResultCreateDto dto);
        public Task<IActionResult> GetExamResultById(Guid examResultId);
        public Task<IActionResult> GetExamsByStudentId(Guid studentId);
        public Task DeleteExamResultById(Guid examResultId);
        public Task<IActionResult> GetExamResultsByExamId(Guid examId);
    }
}
