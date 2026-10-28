using Auth.Model.DTOs.Announcements;

namespace Auth.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Result<AnnouncementCreatedDto>> SendAsync(string senderUserId, CreateAnnouncementDto dto);

        Task<Result<NotificationTargetsDto>> GetTargetsAsync(string userId);

        Task<Result<List<NotificationTargetOption>>> GetSectionStudentsAsync(string userId, Guid sectionId);

        Task<InboxDto> GetInboxAsync(string userId, bool unreadOnly, int page, int pageSize);

        Task<int> GetUnreadCountAsync(string userId);

        Task<Result<bool>> MarkAsReadAsync(string userId, Guid announcementId);

        Task<int> MarkAllAsReadAsync(string userId, IReadOnlyCollection<Guid>? announcementIds);

        Task<List<AnnouncementStatusDto>> GetSentAsync(string userId, int page, int pageSize);

        Task<Result<AnnouncementStatusDto>> GetStatusAsync(string userId, Guid announcementId);

        Task<Result<AnnouncementStatusDto>> RetryAsync(string userId, Guid announcementId);
    }
}
