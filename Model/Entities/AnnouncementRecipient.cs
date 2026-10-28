using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
    /// <summary>
    /// Pending: not yet pushed. Attempted: SignalR send was invoked (no guarantee the client got it).
    /// Acknowledged: the client confirmed receipt through the hub. Failed: gave up after the retry limit.
    /// </summary>
    public enum DeliveryStatus { Pending = 1, Attempted = 2, Acknowledged = 3, Failed = 4 }

    public class AnnouncementRecipient
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid AnnouncementId { get; set; }

        [ForeignKey(nameof(AnnouncementId))]
        public Announcement? Announcement { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required]
        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;

        public int DeliveryAttempts { get; set; }

        public DateTime? LastAttemptAt { get; set; }

        public DateTime? AcknowledgedAt { get; set; }

        [StringLength(500)]
        public string? LastError { get; set; }
    }
}
