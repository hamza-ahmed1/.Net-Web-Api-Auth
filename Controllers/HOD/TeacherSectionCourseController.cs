using Auth.Model.DTOs.TeacherSectionCourse;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.HOD
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherSectionCourseController : ControllerBase
    {
        private readonly ITeacherSectionCourseService _service;

        public TeacherSectionCourseController(ITeacherSectionCourseService service)
        {
            _service = service;
        }

        // GET: api/TeacherSectionCourse
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeacherSectionCourseDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // GET: api/TeacherSectionCourse/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TeacherSectionCourseDto>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result is null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<TeacherSectionCourseDto>> Create([FromBody] TeacherSectionCourseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.TeacherSectionCourseId }, created);
        }

        // PUT: api/TeacherSectionCourse/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TeacherSectionCourseDto>> Update(Guid id, [FromBody] TeacherSectionCourseUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        // DELETE: api/TeacherSectionCourse/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("teacher/{teacherId:guid}")]
        public async Task<ActionResult<IEnumerable<TeacherSectionCourseDto>>> GetAllByTeacherId(Guid teacherId)
        {
            var result = await _service.GetAllByTeacherIdAsync(teacherId);
            return Ok(result);
        }
    }
}
