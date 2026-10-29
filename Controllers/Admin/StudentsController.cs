using System.Security.Claims;
using Auth.Model.DTOs.Student;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
            => await _studentService.CreateStudent(dto);

        [HttpGet]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetAllStudents()
            => Ok(await _studentService.GetAllStudents());

        /// <summary>Accepts a StudentId or user id. Students may only read their own record.</summary>
        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetStudentById(string studentId)
        {
            var student = await _studentService.GetStudentById(studentId);
            if (student == null)
                return NotFound("Student not found.");

            if (User.IsInRole("Student") && !User.IsInRole("Admin") && student.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                return Forbid();

            return Ok(student);
        }

        [HttpPut("{studentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudent(Guid studentId, [FromBody] StudentUpdateDto dto)
            => await _studentService.UpdateStudent(studentId, dto);

        /// <summary>Activates or deactivates a student; inactive students cannot sign in.</summary>
        [HttpPut("{studentId:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetStatus(Guid studentId, [FromBody] StudentStatusDto dto)
            => await _studentService.SetStudentStatus(studentId, dto.IsActive);

        [HttpDelete("{studentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(Guid studentId)
            => await _studentService.DeleteStudent(studentId);

        [HttpGet("section/{sectionId}")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetStudentsBySectionId(Guid sectionId)
            => await _studentService.GetStudentBySectionId(sectionId);

        [HttpGet("student-details/{sectionId}")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetSectionDetails(Guid sectionId)
            => await _studentService.GetstudentDetailsBySectionID(sectionId);

        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportStudentsToCsv()
            => await _studentService.ExportStudentsToExcel();
    }
}
