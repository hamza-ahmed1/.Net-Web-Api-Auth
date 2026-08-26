using Auth.Model.DTOs.Student;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IStudentService
    {
        Task<IActionResult> CreateStudent(StudentCreateDto studentCreateDto);
        Task<List<StudentDto>> GetAllStudents();
        Task<StudentDto?> GetStudentById(Guid studentId);
        // API for getting all students by section ID:
        Task<IActionResult> GetStudentBySectionId(Guid sectionId);
        Task<IActionResult> UpdateStudent(Guid studentId, StudentUpdateDto studentDto);
        Task<IActionResult> DeleteStudent(Guid studentId);
        Task<IActionResult> ExportStudentsToExcel();

    }
}