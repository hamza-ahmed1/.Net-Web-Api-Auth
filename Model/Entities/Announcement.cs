using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
    public enum AudienceType { Teachers = 1, Students = 2, TeachersAndStudents = 3, HODs = 4 }
    public enum Priority { Normal = 1, Important = 2, Urgent = 3 }

    /// <summary>How the recipients were chosen within the audience.</summary>
    public enum RecipientScope { All = 1, Section = 2, Selected = 3 }

    /// <summary>
    /// Background processing state. The announcement row doubles as its own outbox entry:
    /// it is saved as Queued in the same transaction as its recipients, then claimed by the dispatcher.
    /// </summary>
    public enum NotificationStatus { Queued = 1, Processing = 2, Processed = 3, Failed = 4 }

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
        public RecipientScope Scope { get; set; } = RecipientScope.All;

        /// <summary>Target section when Scope is Section (or Selected students within a section).</summary>
        public Guid? SectionId { get; set; }

        [ForeignKey(nameof(SectionId))]
        public Section? Section { get; set; }

        /// <summary>Sender's department at send time, for HOD-scoped notifications.</summary>
        [StringLength(100)]
        public string? Department { get; set; }

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

        public int RecipientCount { get; set; }

        // Dispatch / retry metadata
        public NotificationStatus Status { get; set; } = NotificationStatus.Queued;

        /// <summary>Number of times a worker has claimed this notification.</summary>
        public int Attempts { get; set; }

        /// <summary>Earliest time a queued retry may be claimed.</summary>
        public DateTime? NextAttemptAt { get; set; }

        /// <summary>Lease expiry while Processing; an expired lease means the worker died and the job can be reclaimed.</summary>
        public DateTime? LockedUntil { get; set; }

        [StringLength(100)]
        public string? LockedBy { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [StringLength(2000)]
        public string? LastError { get; set; }

        public ICollection<AnnouncementRecipient> Recipients { get; set; } = new List<AnnouncementRecipient>();
    }
}
