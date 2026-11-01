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
    public class FeeReportController : ControllerBase
    {
        private readonly IFeeReportService _reports;
        private readonly FeeAccessService _access;

        public FeeReportController(IFeeReportService reports, FeeAccessService access)
        {
            _reports = reports;
            _access = access;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] FeeReportQuery filter) =>
            Ok(await _reports.GetSummaryAsync(filter, await _access.GetScopeAsync(User)));

        [HttpGet("defaulters")]
        public async Task<IActionResult> Defaulters([FromQuery] Guid? sectionId, [FromQuery] string? currency, [FromQuery] int page = 1, [FromQuery] int pageSize = 25) =>
            Ok(await _reports.GetDefaultersAsync(sectionId, currency, await _access.GetScopeAsync(User), page, pageSize));

        /// <summary>Students this fee manager may bill, for the fee screens.</summary>
        [HttpGet("students")]
        public async Task<IActionResult> Students() =>
            Ok(await _reports.GetStudentsAsync(await _access.GetScopeAsync(User)));
    }
}
