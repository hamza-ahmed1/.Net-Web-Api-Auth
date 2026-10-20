using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Model.Entities
{
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
    }
}
