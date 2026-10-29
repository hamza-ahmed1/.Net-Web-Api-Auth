using Auth.Model.DTOs.Announcements;
using Auth.Model.DTOs.Lookups;
using Auth.Model.Entities;
using Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Admin
{
    /// <summary>
    /// Shared endpoints for admin-managed lists. Any signed-in user may read them (form dropdowns);
    /// only admins change them.
    /// </summary>
    [ApiController]
    [Authorize]
    public abstract class NamedLookupController<TEntity> : ControllerBase where TEntity : class, INamedLookup, new()
    {
        private readonly NamedLookupService<TEntity> _service;

        protected NamedLookupController(NamedLookupService<TEntity> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = false)
        {
            return Ok(await _service.GetAllAsync(activeOnly));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound(new { message = "Not found." }) : Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] SaveLookupDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value) : ToError(result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaveLookupDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return result.Success ? Ok(result.Value) : ToError(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            return result.Success ? Ok(new { message = "Deleted successfully." }) : ToError(result);
        }

        private IActionResult ToError<T>(Result<T> result)
        {
            var body = new { message = result.Error };
            return result.ErrorKind == ResultErrorKind.NotFound ? NotFound(body) : BadRequest(body);
        }
    }

    [Route("api/cohorts")]
    public class CohortController : NamedLookupController<Cohort>
    {
        public CohortController(NamedLookupService<Cohort> service) : base(service) { }
    }

    [Route("api/campuses")]
    public class CampusController : NamedLookupController<Campus>
    {
        public CampusController(NamedLookupService<Campus> service) : base(service) { }
    }
}
