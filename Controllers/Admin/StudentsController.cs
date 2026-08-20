using Auth.Model.DTOs.Student;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] StudentCreateDto dto)
            => await _studentService.CreateStudent(dto);

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
            => Ok(await _studentService.GetAllStudents());

        [HttpGet("{studentId}")]
        public async Task<IActionResult> GetStudentById(Guid studentId)
        {
            var student = await _studentService.GetStudentById(studentId);
            return student == null ? NotFound("Student not found.") : Ok(student);
        }

        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateStudent(Guid studentId, [FromBody] StudentUpdateDto dto)
            => await _studentService.UpdateStudent(studentId, dto);

        [HttpDelete("{studentId}")]
        public async Task<IActionResult> DeleteStudent(Guid studentId)
            => await _studentService.DeleteStudent(studentId);


        [HttpGet("export")]
        public async Task<IActionResult> ExportStudentsToCsv()
            => await _studentService.ExportStudentsToExcel();
    }
}