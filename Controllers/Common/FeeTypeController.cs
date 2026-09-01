using Auth.Model.DTOs;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeeTypeController : ControllerBase
    {
        private readonly IFeeTypeService _feeTypeService;
        public FeeTypeController(IFeeTypeService feeTypeService)
        {
            _feeTypeService = feeTypeService;
        }
        [HttpPost]

        public async Task<IActionResult> CreateFeeType([FromBody] FeeTypeCreateDto feeTypeDto)
        {
            try
            {
                var result = await _feeTypeService.CreateFeeType(feeTypeDto);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the fee type.", error = ex.Message });
            }

        }
        [HttpGet]

        public async Task<IActionResult> GetAllFeeTypes()
        {
            try
            {
                var result = await _feeTypeService.GetAllFeeTypes();
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving fee types.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeeType(Guid id)
        {
            try
            {
                var result = await _feeTypeService.GetFeeType(id);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving the fee type.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeeType(Guid id, [FromBody] FeeTypeUpdateDto feeTypeDto)
        {
            try
            {
                var result = await _feeTypeService.UpdateFeetype(id, feeTypeDto);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the fee type.", error = ex.Message });
            }
        }


    }



    }
