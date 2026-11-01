using System.Security.Cryptography;
using Auth.Data;
using Auth.Model.DTOs.Announcements;
using Auth.Model.DTOs.Email;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    public class EmailSettingsService : IEmailSettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _protector;
        private readonly IConfiguration _configuration;

        public EmailSettingsService(ApplicationDbContext context, IDataProtectionProvider protection, IConfiguration configuration)
        {
            _context = context;
            _protector = protection.CreateProtector("EmailSettings.AppPassword.v1");
            _configuration = configuration;
        }

        public async Task<EmailSettingsDto> GetAsync()
        {
            var row = await _context.EmailConfigurations.AsNoTracking().FirstOrDefaultAsync();
            return row == null ? new EmailSettingsDto { SmtpPort = 587 } : ToDto(row);
        }

        public async Task<Result<EmailSettingsDto>> UpdateAsync(UpdateEmailSettingsDto dto, string userId)
        {
            var row = await _context.EmailConfigurations.FirstOrDefaultAsync();
            var server = dto.SmtpServer.Trim();
            var senderEmail = dto.SenderEmail.Trim();
            var newPassword = dto.AppPassword?.Trim();
            var hasNewPassword = !string.IsNullOrEmpty(newPassword);

            // The stored password must never be replayed to a different server or account: without this,
            // pointing the settings at another host and pressing Test would hand that host the password.
            var destinationChanged = row != null
                && (!string.Equals(row.SmtpServer, server, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(row.SenderEmail, senderEmail, StringComparison.OrdinalIgnoreCase));
            if (destinationChanged && !hasNewPassword && !string.IsNullOrEmpty(row!.EncryptedAppPassword))
                return Result<EmailSettingsDto>.Failure("You changed the server or sender email, so please enter the app password again.");

            if (row == null)
            {
                row = new EmailConfiguration();
                _context.EmailConfigurations.Add(row);
            }

            row.SmtpServer = server;
            row.SmtpPort = dto.SmtpPort;
            row.SenderEmail = senderEmail;
            row.SenderName = dto.SenderName.Trim();
            if (hasNewPassword)
                row.EncryptedAppPassword = _protector.Protect(newPassword!);
            row.UpdatedAt = DateTime.UtcNow;
            row.UpdatedByUserId = userId;

            await _context.SaveChangesAsync();
            return Result<EmailSettingsDto>.Ok(ToDto(row));
        }

        public async Task<SmtpSettings> GetForSendingAsync()
        {
            var row = await _context.EmailConfigurations.AsNoTracking().FirstOrDefaultAsync();
            var password = row == null ? null : TryDecrypt(row.EncryptedAppPassword);
            if (row == null || string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Email is not set up yet. An administrator must save the app password under Email Setup.");

            return new SmtpSettings(row.SmtpServer, row.SmtpPort, row.SenderEmail, row.SenderName, password);
        }

        public async Task EnsureSeededAsync()
        {
            if (await _context.EmailConfigurations.AnyAsync())
                return;

            var section = _configuration.GetSection("EmailSettings");
            var server = section["SmtpServer"];
            if (string.IsNullOrWhiteSpace(server))
                return;

            // The password is only imported if supplied outside source control (environment/user secrets);
            // otherwise the administrator enters it on the Email Setup page.
            var password = section["AppPassword"];
            _context.EmailConfigurations.Add(new EmailConfiguration
            {
                SmtpServer = server,
                SmtpPort = int.TryParse(section["SmtpPort"], out var port) ? port : 587,
                SenderEmail = section["SenderEmail"] ?? string.Empty,
                SenderName = section["SenderName"] ?? string.Empty,
                EncryptedAppPassword = string.IsNullOrWhiteSpace(password) ? null : _protector.Protect(password)
            });
            await _context.SaveChangesAsync();
        }

        private EmailSettingsDto ToDto(EmailConfiguration row) => new()
        {
            SmtpServer = row.SmtpServer,
            SmtpPort = row.SmtpPort,
            SenderEmail = row.SenderEmail,
            SenderName = row.SenderName,
            HasPassword = !string.IsNullOrEmpty(TryDecrypt(row.EncryptedAppPassword)),
            UpdatedAt = DateTime.SpecifyKind(row.UpdatedAt, DateTimeKind.Utc)
        };

        // Encryption keys can be lost (e.g. a new server); the admin then simply re-enters the password
        private string? TryDecrypt(string? encrypted)
        {
            if (string.IsNullOrEmpty(encrypted))
                return null;
            try
            {
                return _protector.Unprotect(encrypted);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }
    }
}
