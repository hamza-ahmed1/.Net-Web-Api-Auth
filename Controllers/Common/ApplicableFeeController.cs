using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicableFeeController : ControllerBase
    {
        private readonly IApplicableFeeService _applicableFeeService;

        public ApplicableFeeController(IApplicableFeeService applicableFeeService)
        {
            _applicableFeeService = applicableFeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplicableFees()
        {
            try
            {
                return await _applicableFeeService.GetAllApplicableFees();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            
            
        }
        [HttpPost]
        public async Task<IActionResult> CreateApplicableFee([FromBody] Model.DTOs.ApplicableFeeDto applicableFeeDto)

        {
            try
            {
                var result = await _applicableFeeService.CreateApplicableFee(applicableFeeDto);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicableFeeById(Guid id)
        {
            try
            {

                return await _applicableFeeService.GetApplicableFeesById(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetApplicableFeesByStudentId(Guid studentId)
        {
            try
            {
                return await _applicableFeeService.GetApplicableFeesByStudentId(studentId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApplicableFee(Guid id, [FromBody] Model.DTOs.ApplicableFeeDto applicableFeeDto)
        {
            try
            {
                return await _applicableFeeService.UpdateApplicableFee(id, applicableFeeDto);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
