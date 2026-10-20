using Auth.Model.Entities;

namespace Auth.Model.DTOs.Announcements
{
    public class AnnouncementDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public Priority Priority { get; set; }
        public AudienceType Audience { get; set; }
        public string SenderRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
