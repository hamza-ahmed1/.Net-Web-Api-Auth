using Auth.Model.DTOs.Teachers;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/teachers")]
    public class TeachersController:ControllerBase
    {

        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }


        [HttpPost("create")]

        public async Task<IActionResult> CreateTeacher(CreateTeacherDto Dto)
        {
            return await _teacherService.RegisterTeacher(Dto);
        }
    }
}
