using System.Security.Claims;
using Auth.Model.DTOs.Announcements;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers.Common
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        /// <summary>Queues a notification and returns 202 without waiting for real-time delivery.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> Send([FromBody] CreateAnnouncementDto dto)
        {
            var result = await _notificationService.SendAsync(CurrentUserId, dto);
            return result.Success
                ? AcceptedAtAction(nameof(GetStatus), new { id = result.Value!.Id }, result.Value)
                : ToError(result);
        }

        [HttpGet("targets")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetTargets()
        {
            var result = await _notificationService.GetTargetsAsync(CurrentUserId);
            return result.Success ? Ok(result.Value) : ToError(result);
        }

        [HttpGet("targets/sections/{sectionId:guid}/students")]
        [Authorize(Roles = "HOD,Teacher")]
        public async Task<IActionResult> GetSectionStudents(Guid sectionId)
        {
            var result = await _notificationService.GetSectionStudentsAsync(CurrentUserId, sectionId);
            return result.Success ? Ok(result.Value) : ToError(result);
        }

        /// <summary>The caller's own notifications, newest first.</summary>
        [HttpGet]
        public async Task<IActionResult> GetInbox([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return Ok(await _notificationService.GetInboxAsync(CurrentUserId, unreadOnly, page, pageSize));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            return Ok(new { count = await _notificationService.GetUnreadCountAsync(CurrentUserId) });
        }

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var result = await _notificationService.MarkAsReadAsync(CurrentUserId, id);
            return result.Success ? NoContent() : ToError(result);
        }

        /// <summary>Marks the given notifications as read, or all of them when no ids are sent.</summary>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead([FromBody] MarkReadRequest? request)
        {
            var updated = await _notificationService.MarkAllAsReadAsync(CurrentUserId, request?.Ids);
            return Ok(new { updated });
        }

        /// <summary>Notifications the caller sent (administrators see all) with processing status.</summary>
        [HttpGet("sent")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetSent([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return Ok(await _notificationService.GetSentAsync(CurrentUserId, page, pageSize));
        }

        [HttpGet("{id:guid}/status")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> GetStatus(Guid id)
        {
            var result = await _notificationService.GetStatusAsync(CurrentUserId, id);
            return result.Success ? Ok(result.Value) : ToError(result);
        }

        [HttpPost("{id:guid}/retry")]
        [Authorize(Roles = "Admin,HOD,Teacher")]
        public async Task<IActionResult> Retry(Guid id)
        {
            var result = await _notificationService.RetryAsync(CurrentUserId, id);
            return result.Success ? Ok(result.Value) : ToError(result);
        }

        private IActionResult ToError<T>(Result<T> result)
        {
            var body = new { message = result.Error };
            return result.ErrorKind switch
            {
                ResultErrorKind.Forbidden => StatusCode(StatusCodes.Status403Forbidden, body),
                ResultErrorKind.NotFound => NotFound(body),
                _ => BadRequest(body)
            };
        }

        public class MarkReadRequest
        {
            public List<Guid>? Ids { get; set; }
        }
    }
}
