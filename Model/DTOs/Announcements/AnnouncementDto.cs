namespace Auth.Model.DTOs.Announcements
{
    /// <summary>A notification as seen by one recipient (inbox item and SignalR payload).</summary>
    public class AnnouncementDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class InboxDto
    {
        public List<AnnouncementDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int UnreadCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class AnnouncementCreatedDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public int RecipientCount { get; set; }
    }

    /// <summary>Processing and delivery summary for the sender or an administrator.</summary>
    public class AnnouncementStatusDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string? Section { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Attempts { get; set; }
        public DateTime? NextAttemptAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? LastError { get; set; }
        public int RecipientCount { get; set; }
        public int Pending { get; set; }
        public int Attempted { get; set; }
        public int Acknowledged { get; set; }
        public int Failed { get; set; }
        public int ReadCount { get; set; }
    }

    public class NotificationTargetOption
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string? Detail { get; set; }
    }

    public class NotificationAudienceOption
    {
        public string Audience { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new();
    }

    /// <summary>What the current user may target, computed server-side from their roles and assignments.</summary>
    public class NotificationTargetsDto
    {
        public string SenderRole { get; set; } = string.Empty;
        public string? Department { get; set; }
        public List<NotificationAudienceOption> Audiences { get; set; } = new();
        public List<NotificationTargetOption> Sections { get; set; } = new();
        public List<NotificationTargetOption> Teachers { get; set; } = new();
        public List<NotificationTargetOption> Hods { get; set; } = new();
    }
}
