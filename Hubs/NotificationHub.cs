using Auth.Data;
using Auth.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Auth.Hubs
{
    /// <summary>
    /// Real-time notification delivery. Clients only listen; the server targets users by the
    /// authenticated NameIdentifier claim (SignalR's default user id), never by client-supplied ids or groups.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        public const string Path = "/hubs/notifications";

        /// <summary>Server-to-client event carrying an AnnouncementDto.</summary>
        public const string ReceiveNotification = "ReceiveNotification";

        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ApplicationDbContext context, ILogger<NotificationHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogDebug("Notification connection {ConnectionId} opened for user {UserId}", Context.ConnectionId, Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogDebug(exception, "Notification connection {ConnectionId} closed for user {UserId}", Context.ConnectionId, Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client confirms it received a pushed notification. Only the caller's own recipient row can change,
        /// so a client cannot acknowledge on behalf of anyone else.
        /// </summary>
        public async Task AcknowledgeNotification(Guid notificationId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
                return;

            await _context.AnnouncementRecipients
                .Where(r => r.AnnouncementId == notificationId && r.UserId == userId && r.AcknowledgedAt == null)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.DeliveryStatus, DeliveryStatus.Acknowledged)
                    .SetProperty(r => r.AcknowledgedAt, DateTime.UtcNow));
        }
    }
}
