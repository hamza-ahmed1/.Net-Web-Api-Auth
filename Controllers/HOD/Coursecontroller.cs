using Auth.Model.DTOs.Courses;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.HOD
{
    [ApiController]
    [Route("api/courses")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateCourseDto dto)
        {
            var course = await _courseService.CreateCourse(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = course.CourseId },
                course
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _courseService.GetAllCourses();

            return Ok(courses);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var course = await _courseService.GetCourseById(id);

            if (course == null)
            {
                return NotFound(new
                {
                    message = "Course not found."
                });
            }

            return Ok(course);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] CreateCourseDto dto)
        {
            var course = await _courseService.UpdateCourse(id, dto);

            if (course == null)
            {
                return NotFound(new
                {
                    message = "Course not found."
                });
            }

            return Ok(course);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _courseService.DeleteCourse(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Course not found."
                });
            }

            return Ok(new
            {
                message = "Course deleted successfully."
            });
        }
    }
}
