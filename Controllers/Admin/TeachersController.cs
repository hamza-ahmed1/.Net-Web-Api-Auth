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


        [HttpPost("edit")]

        public async Task<IActionResult> UpdateTeacher(TeacherDetailDto Dto)
        {
            return await _teacherService.UpdateTeacherDetails(Dto);
        }

        [HttpGet("{teacherId}")]

        public async Task<TeacherDetailDto> GetTeacherById(Guid teacherId)
        {
            try
            {
                var result = await _teacherService.GetTeacherByID(teacherId);
                return new TeacherDetailDto
                {
                    Address = result.Address,
                    Fullname = result.User.FullName,
                    IsActive = result.IsActive,
                    CNIC = result.CNIC,
                    DateOfBirth = result.DateOfBirth,
                    Department = result.Department,
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

    }
}
