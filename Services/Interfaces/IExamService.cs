using Auth.Model.DTOs.Exam;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface IExamService
    {
        public Task<IActionResult> CreateExam(CreateExamDto examDto);
        public Task<IActionResult> GetAllExamByTeacherSection(Guid id);
        public Task<IActionResult> UpdateExam(Guid examId, CreateExamDto examDto);
        public Task<IActionResult> DeleteExam(Guid examId);

        public Task<IActionResult> GetExamByTeacherId(Guid teacher_id);


    }
}
