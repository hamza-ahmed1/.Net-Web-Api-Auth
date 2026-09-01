using Auth.Model.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IApplicableFeeService
    {
        public Task<IActionResult> CreateApplicableFee(ApplicableFeeDto applicableFeeDto);
        public Task<IActionResult> GetAllApplicableFees();
        public Task<IActionResult> GetApplicableFeesById(Guid id);
        public Task<IActionResult> GetApplicableFeesByStudentId(Guid studentId);
        public Task<IActionResult> UpdateApplicableFee(Guid id, ApplicableFeeDto applicableFeeDto);
    }
}
