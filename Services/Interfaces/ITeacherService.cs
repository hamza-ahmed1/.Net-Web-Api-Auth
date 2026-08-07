using Auth.Model.DTOs.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface ITeacherService
    {
        public Task<IActionResult> RegisterTeacher(CreateTeacherDto dto);
        public Task<IActionResult> UpdateTeacherDetails(TeacherDetailDto dto);

        public Task<IActionResult> DeleteTeacher(Guid teacherId);

        public Task<Teacher> GetTeacherByID(Guid teacher_id);
        public Task<bool> RestoreTeacher(Guid teacherId);
        public Task<IActionResult> PromoteToHOD(Guid teacherId);
        public Task<IActionResult> DemoteToTeacher(Guid teacherId);

    }
}
