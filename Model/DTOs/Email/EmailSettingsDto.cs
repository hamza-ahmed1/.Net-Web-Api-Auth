using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs.Email
{
    /// <summary>What the Admin Panel sees. There is deliberately no password field here.</summary>
    public class EmailSettingsDto
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        /// <summary>True when a usable password is stored; the UI shows a masked placeholder.</summary>
        public bool HasPassword { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateEmailSettingsDto
    {
        [Required(ErrorMessage = "SMTP server is required.")]
        [StringLength(200)]
        [RegularExpression(@"^[A-Za-z0-9]([A-Za-z0-9\-\.]*[A-Za-z0-9])?$", ErrorMessage = "Enter a valid server name, e.g. smtp.gmail.com.")]
        public string SmtpServer { get; set; } = string.Empty;

        [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
        public int SmtpPort { get; set; } = 587;

        [Required(ErrorMessage = "Sender email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid sender email.")]
        [StringLength(256)]
        public string SenderEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sender name is required.")]
        [StringLength(100)]
        public string SenderName { get; set; } = string.Empty;

        /// <summary>Leave empty to keep the saved password.</summary>
        [StringLength(256)]
        public string? AppPassword { get; set; }
    }

    public class TestEmailDto
    {
        [Required(ErrorMessage = "Enter the address to send the test to.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string RecipientEmail { get; set; } = string.Empty;
    }

    /// <summary>Decrypted settings for sending mail. Server-side only: never returned by an API.</summary>
    public sealed record SmtpSettings(string SmtpServer, int SmtpPort, string SenderEmail, string SenderName, string AppPassword);
}
