using Auth.Model.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IApplicableFeeService
    {
        public Task<IActionResult> CreateApplicableFee(ApplicableFeeDto applicableFeeDto, FeeScope scope);
        public Task<IActionResult> ApplyFeeBulk(List<ApplicableFeeDto> applicableFeeDtos, FeeScope scope);
        public Task<IActionResult> GetAllApplicableFees(FeeScope scope);
        public Task<IActionResult> GetApplicableFeesById(Guid id, FeeScope scope);
        public Task<IActionResult> GetApplicableFeesByStudentId(Guid studentId, FeeScope scope);
        public Task<IActionResult> GetBillableFeesForStudent(Guid studentId, FeeScope scope);
        public Task<IActionResult> GetbulkApplicableFeeBySectionId(Guid sectionId, FeeScope scope);
        public Task<IActionResult> UpdateApplicableFee(Guid id, ApplicableFeeDto applicableFeeDto, FeeScope scope);
        public Task<IActionResult> DeleteApplicableFee(Guid id, FeeScope scope);
    }
}
