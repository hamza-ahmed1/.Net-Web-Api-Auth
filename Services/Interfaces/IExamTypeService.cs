using Auth.Model.DTOs;
using Auth.Model.DTOs.Exam;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IExamTypeService
    {
        public Task<IActionResult> CreateExamType(ExamTypeDTO dto);
        public Task<IActionResult> UpdateExamType(Guid id, ExamTypeDTO dto);
        public Task<IActionResult> GetAll();
    }
}
