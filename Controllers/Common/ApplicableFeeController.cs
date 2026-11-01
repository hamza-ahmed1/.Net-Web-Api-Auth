using Auth.Model.DTOs;
using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = FeeRules.Staff)]
    public class ApplicableFeeController : ControllerBase
    {
        private readonly IApplicableFeeService _applicableFeeService;
        private readonly FeeAccessService _access;

        public ApplicableFeeController(IApplicableFeeService applicableFeeService, FeeAccessService access)
        {
            _applicableFeeService = applicableFeeService;
            _access = access;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplicableFees() =>
            await _applicableFeeService.GetAllApplicableFees(await _access.GetScopeAsync(User));

        [HttpPost]
        public async Task<IActionResult> CreateApplicableFee([FromBody] ApplicableFeeDto applicableFeeDto) =>
            await _applicableFeeService.CreateApplicableFee(applicableFeeDto, await _access.GetScopeAsync(User));

        [HttpPost("bulk")]
        public async Task<IActionResult> ApplyFeeBulk([FromBody] List<ApplicableFeeDto> applicableFeeDtos) =>
            await _applicableFeeService.ApplyFeeBulk(applicableFeeDtos, await _access.GetScopeAsync(User));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicableFeeById(Guid id) =>
            await _applicableFeeService.GetApplicableFeesById(id, await _access.GetScopeAsync(User));

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetApplicableFeesByStudentId(Guid studentId) =>
            await _applicableFeeService.GetApplicableFeesByStudentId(studentId, await _access.GetScopeAsync(User));

        // get bulk fees by section ID;
        [HttpGet("section/{sectionId}")]
        public async Task<IActionResult> GetbulkApplicableFeeBySectionId(Guid sectionId) =>
            await _applicableFeeService.GetbulkApplicableFeeBySectionId(sectionId, await _access.GetScopeAsync(User));

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApplicableFee(Guid id, [FromBody] ApplicableFeeDto applicableFeeDto) =>
            await _applicableFeeService.UpdateApplicableFee(id, applicableFeeDto, await _access.GetScopeAsync(User));

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplicableFee(Guid id) =>
            await _applicableFeeService.DeleteApplicableFee(id, await _access.GetScopeAsync(User));
    }
}
