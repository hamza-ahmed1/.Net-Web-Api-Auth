using System.ComponentModel.DataAnnotations;

namespace Auth.Model.DTOs.Announcements
{
    public class CreateAnnouncementDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>Normal, Important or Urgent.</summary>
        public string Priority { get; set; } = "Normal";

        /// <summary>HODs, Teachers or Students. Allowed values depend on the sender's role.</summary>
        [Required]
        public string Audience { get; set; } = string.Empty;

        /// <summary>All, Section or Selected.</summary>
        [Required]
        public string Scope { get; set; } = "All";

        /// <summary>Required for Section scope; optional filter for Selected students.</summary>
        public Guid? SectionId { get; set; }

        /// <summary>
        /// For Selected scope: Teacher_Id values (HODs/Teachers audience) or StudentId values (Students audience).
        /// Every id must be within the sender's eligible recipients.
        /// </summary>
        public List<Guid> RecipientIds { get; set; } = new();

        public DateTime? ExpiresAt { get; set; }
    }
}
