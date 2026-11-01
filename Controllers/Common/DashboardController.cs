using System.Security.Claims;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboard;

        public DashboardController(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin() => Ok(await _dashboard.GetAdminAsync());

        [HttpGet("hod")]
        [Authorize(Roles = "HOD")]
        public async Task<IActionResult> Hod()
        {
            var dashboard = await _dashboard.GetHodAsync(UserId);
            return dashboard == null
                ? NotFound(new { message = "Your account is not linked to a department yet." })
                : Ok(dashboard);
        }

        [HttpGet("teacher")]
        [Authorize(Roles = "Teacher,HOD")]
        public async Task<IActionResult> Teacher()
        {
            var dashboard = await _dashboard.GetTeacherAsync(UserId);
            return dashboard == null
                ? NotFound(new { message = "Your account has no teacher profile." })
                : Ok(dashboard);
        }
    }
}
