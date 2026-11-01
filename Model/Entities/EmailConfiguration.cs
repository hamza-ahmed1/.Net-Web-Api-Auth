using System.ComponentModel.DataAnnotations;

namespace Auth.Model.Entities
{
    /// <summary>
    /// SMTP settings managed from the Admin Panel (single row). The app password is stored encrypted with
    /// ASP.NET Data Protection and never leaves the server.
    /// </summary>
    public class EmailConfiguration
    {
        public const int SingletonId = 1;

        [Key]
        [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
        public int Id { get; set; } = SingletonId;

        [Required]
        [StringLength(200)]
        public string SmtpServer { get; set; } = string.Empty;

        public int SmtpPort { get; set; } = 587;

        [Required]
        [StringLength(256)]
        public string SenderEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SenderName { get; set; } = string.Empty;

        /// <summary>Encrypted app password; null until one has been entered.</summary>
        public string? EncryptedAppPassword { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? UpdatedByUserId { get; set; }
    }
}
