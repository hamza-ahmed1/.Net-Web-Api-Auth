using Auth.Model.DTOs.Announcements;
using Auth.Model.DTOs.Departments;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Admin
{
    [ApiController]
    [Route("api/departments")]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        /// <summary>Any signed-in user may read departments (used by dropdowns); only admins change them.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            return Ok(await _departmentService.GetAllAsync(activeOnly));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            return department == null ? NotFound(new { message = "Department not found." }) : Ok(department);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] SaveDepartmentDto dto)
        {
            var result = await _departmentService.CreateAsync(dto);
            return result.Success
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.DepartmentId }, result.Value)
                : ToError(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveDepartmentDto dto)
        {
            var result = await _departmentService.UpdateAsync(id, dto);
            return result.Success ? Ok(result.Value) : ToError(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _departmentService.DeleteAsync(id);
            return result.Success ? Ok(new { message = "Department deleted successfully." }) : ToError(result);
        }

        private IActionResult ToError<T>(Result<T> result)
        {
            var body = new { message = result.Error };
            return result.ErrorKind switch
            {
                ResultErrorKind.Forbidden => StatusCode(StatusCodes.Status403Forbidden, body),
                ResultErrorKind.NotFound => NotFound(body),
                _ => BadRequest(body)
            };
        }
    }
}
