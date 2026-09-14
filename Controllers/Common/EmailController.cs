using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Auth.Model.DTOs;

namespace Auth.Controllers.Common
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail([FromBody] EmailDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.RecipientEmail) || string.IsNullOrEmpty(request.Subject) || string.IsNullOrEmpty(request.HtmlMessage))
            {
                return BadRequest("Invalid email request.");
            }
            try
            {
                await _emailService.SendEmailAsync(request.RecipientEmail, request.Subject, request.HtmlMessage);
                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here as needed
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error sending email: {ex.Message}");
            }
        }

    }
}
