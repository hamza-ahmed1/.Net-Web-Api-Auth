using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
    public enum AudienceType { Teachers = 1, Students = 2, TeachersAndStudents = 3 }
    public enum Priority { Normal = 1, Important = 2, Urgent = 3 }

    public class Announcement
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        [Required]
        public Priority Priority { get; set; } = Priority.Normal;

        [Required]
        public AudienceType Audience { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [ForeignKey(nameof(SenderId))]
        public ApplicationUser? Sender { get; set; }

        [Required]
        [StringLength(20)]
        public string SenderRole { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public ICollection<AnnouncementRecipient> Recipients { get; set; } = new List<AnnouncementRecipient>();
    }
}
