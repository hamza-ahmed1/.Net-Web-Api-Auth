using Auth.Model.DTOs.Announcements;
using Auth.Model.DTOs.Email;

namespace Auth.Services.Interfaces
{
    public interface IEmailSettingsService
    {
        Task<EmailSettingsDto> GetAsync();

        Task<Result<EmailSettingsDto>> UpdateAsync(UpdateEmailSettingsDto dto, string userId);

        /// <summary>Decrypted settings for EmailService; throws when email has not been set up.</summary>
        Task<SmtpSettings> GetForSendingAsync();

        /// <summary>First run: creates the settings row from the EmailSettings configuration section.</summary>
        Task EnsureSeededAsync();
    }
}
