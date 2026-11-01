using System.Security.Claims;
using Auth.Model.DTOs.Email;
using Auth.Services.Interfaces;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/email-settings")]
    [Authorize(Roles = "Admin")]
    public class EmailSettingsController : ControllerBase
    {
        private readonly IEmailSettingsService _settings;
        private readonly IEmailService _email;
        private readonly ILogger<EmailSettingsController> _logger;

        public EmailSettingsController(IEmailSettingsService settings, IEmailService email, ILogger<EmailSettingsController> logger)
        {
            _settings = settings;
            _email = email;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _settings.GetAsync());

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEmailSettingsDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var result = await _settings.UpdateAsync(dto, userId);
            return result.Success ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        /// <summary>Sends a test message using the saved settings.</summary>
        [HttpPost("test")]
        public async Task<IActionResult> SendTest([FromBody] TestEmailDto dto)
        {
            const string body = "<p>This is a test message from <strong>TCF Campus</strong>.</p><p>If you can read this, the email settings are working.</p>";
            try
            {
                await _email.SendEmailAsync(dto.RecipientEmail, "TCF Campus test email", body);
                return Ok(new { message = $"Test email sent to {dto.RecipientEmail}." });
            }
            catch (Exception ex) when (ex is InvalidOperationException or AuthenticationException or SmtpCommandException or SmtpProtocolException or SslHandshakeException or System.Net.Sockets.SocketException or TimeoutException or OperationCanceledException or IOException)
            {
                _logger.LogWarning(ex, "Test email failed");
                return BadRequest(new { message = Explain(ex) });
            }
        }

        private static string Explain(Exception ex) => ex switch
        {
            InvalidOperationException => ex.Message,
            AuthenticationException => "The mail server rejected the sender email or app password.",
            SslHandshakeException => "A secure connection could not be made. Check the server and port.",
            SmtpCommandException => "The mail server refused the message. Check the sender and recipient addresses.",
            _ => "Could not reach the mail server. Check the server name, port and internet connection."
        };
    }
}
