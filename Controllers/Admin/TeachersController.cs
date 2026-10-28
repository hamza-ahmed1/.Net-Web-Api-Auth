using Auth.Model.DTOs.Teachers;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/teachers")]
    [Authorize]
    public class TeachersController:ControllerBase
    {

        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }


        [HttpPost]

        public async Task<IActionResult> CreateTeacher(CreateTeacherDto Dto)
        {
            return await _teacherService.RegisterTeacher(Dto);
        }

        [HttpGet]
        public async Task<List<TeacherDetailDto>> GetAllTeachers()
        {
            return await _teacherService.GetAllTechers();
        }
        [HttpPut]

        public async Task<IActionResult> UpdateTeacher(TeacherDetailDto Dto)
        {
            return await _teacherService.UpdateTeacherDetails(Dto);
        }

        [HttpGet("{teacherId}")]

        public async Task<TeacherDetailDto> GetTeacherById(Guid teacherId)
        {
            try
            {
                var teachers = await _teacherService.GetAllTechers();
                var match = teachers.FirstOrDefault(t => t.Teacher_id == teacherId);
                if (match != null)
                    return match;

                var result = await _teacherService.GetTeacherByID(teacherId);
                return new TeacherDetailDto
                {
                    Teacher_id = result.Teacher_Id,
                    Address = result.Address,
                    Fullname = result.User.FullName,
                    IsActive = result.IsActive,
                    CNIC = result.CNIC,
                    DateOfBirth = result.DateOfBirth,
                    DepartmentId = result.DepartmentId,
                    Department = result.Department?.Name ?? string.Empty,
                    Salary = result.Salary,
                    Email = result?.User.Email,
                    HireDate = result.HireDate,
                    IdentificationNumber = result.IdentificationNumber,
                    Qualification = result.Qualification
                };

            }
            catch (Exception ex)
            {
                
                throw new Exception("An error occurred while retrieving the teacher details.", ex);
            }


        }


        [HttpDelete("{teacherId}")]

        public async Task<IActionResult> DeleteTeacherById(Guid teacherId)
        {
            try
            {
                return await _teacherService.DeleteTeacher(teacherId);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting!", ex);

            }
        }

        [HttpPost("restore/{teacherId}")]
        public async Task<IActionResult> RestoreTeacherById(Guid teacherId)
        {
            try
            {
                var teacher = await _teacherService.RestoreTeacher(teacherId);
                return Ok(teacher);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while restoring the teacher.", ex);
            }
        }

        // endpoint to promote teacher to HOD

        [HttpPost("promotion/{teacherId}")]
        public async Task<IActionResult> TeacherToHOD(Guid teacherId)
        {
            return await _teacherService.PromoteToHOD(teacherId);
        }

        [HttpPost("demotion/{teacherId}")]
        public async Task<IActionResult> DemoteToTeacher(Guid teacherId)
        {
            try
            {
                return await _teacherService.DemoteToTeacher(teacherId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportTeachers()
        {
            return await _teacherService.ExportTeachers();
        }

    }
}
