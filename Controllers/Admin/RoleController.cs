using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Admin
{
    [ApiController]
    [Route("api/roles")]
    public class RoleController : ControllerBase
    {

        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }


        [HttpGet]
        public async Task<IActionResult> Get() {
            return await _roleService.GetAllRoles();
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] string roleNameobj)
        {
            return await _roleService.CreateRole(roleNameobj);
        }
    }
}
