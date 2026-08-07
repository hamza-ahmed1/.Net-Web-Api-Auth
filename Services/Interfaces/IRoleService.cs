using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IRoleService
    {
        public Task<IActionResult> CreateRole(string roleName);

        public Task<IActionResult> GetAllRoles();
               
    }
}
