using Auth.Model.DTOs.Teachers;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Interfaces
{
    public interface ITeacherService
    {
        public Task<IActionResult> RegisterTeacher(CreateTeacherDto dto);
    }
}
