using Auth.Data;
using Auth.Model.DTOs.Announcements;
using Auth.Model.Entities;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Services
{
    /// <summary>
    /// Role-based notifications, stored as Announcement + AnnouncementRecipient rows.
    /// Sending only validates, resolves recipients and saves everything as Queued in one transaction;
    /// NotificationDispatcher does the real-time delivery in the background.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private const int MaxPageSize = 100;

        // Which audiences and scopes each sender role may use
        private static readonly Dictionary<string, Dictionary<AudienceType, RecipientScope[]>> Permissions = new()
        {
            ["Admin"] = new()
            {
                [AudienceType.HODs] = [RecipientScope.All, RecipientScope.Selected],
                [AudienceType.Teachers] = [RecipientScope.All, RecipientScope.Selected],
            },
            ["HOD"] = new()
            {
                [AudienceType.Teachers] = [RecipientScope.All, RecipientScope.Selected],
                [AudienceType.Students] = [RecipientScope.All, RecipientScope.Section, RecipientScope.Selected],
            },
            ["Teacher"] = new()
            {
                [AudienceType.Students] = [RecipientScope.All, RecipientScope.Section, RecipientScope.Selected],
            },
        };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationQueueSignal _queueSignal;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            NotificationQueueSignal queueSignal,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _userManager = userManager;
            _queueSignal = queueSignal;
            _logger = logger;
        }

        private sealed record Sender(ApplicationUser User, string Role, Teacher? Teacher)
        {
            public string? Department => Teacher?.Department?.Name;

            public Guid? DepartmentId => Teacher?.DepartmentId;
        }

        /// <summary>Entity id (Teacher_Id or StudentId) mapped to the Identity user it notifies.</summary>
        private sealed record Candidate(Guid Id, string UserId, string Name, string? Detail);

        // SEND

        public async Task<Result<AnnouncementCreatedDto>> SendAsync(string senderUserId, CreateAnnouncementDto dto)
        {
            var sender = await GetSenderAsync(senderUserId);
            if (sender == null)
                return Result<AnnouncementCreatedDto>.Forbidden("You are not allowed to send notifications.");

            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Message))
                return Result<AnnouncementCreatedDto>.Failure("Title and message are required.");

            if (!TryParseEnum<AudienceType>(dto.Audience, out var audience))
                return Result<AnnouncementCreatedDto>.Failure("Audience must be HODs, Teachers or Students.");
            if (!TryParseEnum<RecipientScope>(dto.Scope, out var scope))
                return Result<AnnouncementCreatedDto>.Failure("Scope must be All, Section or Selected.");
            if (!TryParseEnum<Priority>(string.IsNullOrWhiteSpace(dto.Priority) ? "Normal" : dto.Priority, out var priority))
                return Result<AnnouncementCreatedDto>.Failure("Priority must be Normal, Important or Urgent.");

            var now = DateTime.UtcNow;
            if (dto.ExpiresAt.HasValue && dto.ExpiresAt.Value <= now)
                return Result<AnnouncementCreatedDto>.Failure("Expiry must be in the future.");

            if (!Permissions[sender.Role].TryGetValue(audience, out var allowedScopes) || !allowedScopes.Contains(scope))
                return Result<AnnouncementCreatedDto>.Forbidden($"{sender.Role} users cannot send to {audience} with {scope} scope.");

            if (scope == RecipientScope.Section && !dto.SectionId.HasValue)
                return Result<AnnouncementCreatedDto>.Failure("Choose a section.");
            if (scope == RecipientScope.Selected && (dto.RecipientIds == null || dto.RecipientIds.Count == 0))
                return Result<AnnouncementCreatedDto>.Failure("Select at least one recipient.");

            // Section narrows Section and Selected scopes; All ignores it
            var sectionId = scope == RecipientScope.All ? null : dto.SectionId;
            if (sectionId.HasValue)
            {
                if (audience != AudienceType.Students)
                    return Result<AnnouncementCreatedDto>.Failure("Sections apply only to student notifications.");

                var allowedSections = await GetAllowedSectionIdsAsync(sender);
                if (!allowedSections.Contains(sectionId.Value))
                    return Result<AnnouncementCreatedDto>.Forbidden("You are not allowed to notify this section.");
            }

            var candidates = await GetCandidatesAsync(sender, audience, sectionId);
            var recipients = candidates;
            if (scope == RecipientScope.Selected)
            {
                var requested = dto.RecipientIds!.ToHashSet();
                recipients = candidates.Where(c => requested.Contains(c.Id)).ToList();
                if (recipients.Select(r => r.Id).Distinct().Count() != requested.Count)
                    return Result<AnnouncementCreatedDto>.Forbidden("One or more selected recipients are outside your allowed scope.");
            }

            var userIds = recipients
                .Select(r => r.UserId)
                .Where(id => id != senderUserId)
                .Distinct()
                .ToList();

            if (userIds.Count == 0)
                return Result<AnnouncementCreatedDto>.Failure("No eligible recipients were found.");

            // Recipients are a snapshot taken now: students who join the section later do not receive it,
            // and students who leave keep it in their inbox.
            var announcement = new Announcement
            {
                Id = Guid.NewGuid(),
                Title = dto.Title.Trim(),
                Body = dto.Message.Trim(),
                Priority = priority,
                Audience = audience,
                Scope = scope,
                SectionId = sectionId,
                Department = sender.Role == "HOD" ? sender.Department : null,
                SenderId = senderUserId,
                SenderRole = sender.Role,
                CreatedAt = now,
                ExpiresAt = dto.ExpiresAt,
                RecipientCount = userIds.Count,
                Status = NotificationStatus.Queued,
                Recipients = userIds
                    .Select(userId => new AnnouncementRecipient { Id = Guid.NewGuid(), UserId = userId })
                    .ToList()
            };

            // Notification, recipients and its Queued (outbox) state commit together in one transaction
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            _queueSignal.Notify();

            _logger.LogInformation(
                "Notification {NotificationId} queued by {SenderRole} {SenderId} for {RecipientCount} {Audience} ({Scope})",
                announcement.Id, sender.Role, senderUserId, userIds.Count, audience, scope);

            return Result<AnnouncementCreatedDto>.Ok(new AnnouncementCreatedDto
            {
                Id = announcement.Id,
                Status = announcement.Status.ToString(),
                RecipientCount = userIds.Count
            });
        }

        // TARGETS (what the UI may offer; the same rules are re-checked on send)

        public async Task<Result<NotificationTargetsDto>> GetTargetsAsync(string userId)
        {
            var sender = await GetSenderAsync(userId);
            if (sender == null)
                return Result<NotificationTargetsDto>.Forbidden("You are not allowed to send notifications.");

            var targets = new NotificationTargetsDto
            {
                SenderRole = sender.Role,
                Department = sender.Role == "HOD" ? sender.Department : null,
                Audiences = Permissions[sender.Role]
                    .Select(p => new NotificationAudienceOption
                    {
                        Audience = p.Key.ToString(),
                        Scopes = p.Value.Select(s => s.ToString()).ToList()
                    })
                    .ToList()
            };

            if (sender.Role == "Admin")
            {
                targets.Hods = ToOptions(await GetTeacherCandidatesAsync(hods: true, departmentId: null));
                targets.Teachers = ToOptions(await GetTeacherCandidatesAsync(hods: false, departmentId: null));
            }
            else
            {
                if (sender.Role == "HOD")
                    targets.Teachers = ToOptions((await GetTeacherCandidatesAsync(hods: false, sender.DepartmentId)).Where(c => c.UserId != userId));

                var sectionIds = await GetAllowedSectionIdsAsync(sender);
                targets.Sections = await _context.Sections
                    .AsNoTracking()
                    .Where(s => sectionIds.Contains(s.SectionId))
                    .OrderBy(s => s.IntermediateClass)
                    .ThenBy(s => s.SectionName)
                    .Select(s => new NotificationTargetOption
                    {
                        Id = s.SectionId,
                        Label = s.IntermediateClass + " - " + s.SectionName
                    })
                    .ToListAsync();
            }

            return Result<NotificationTargetsDto>.Ok(targets);
        }

        public async Task<Result<List<NotificationTargetOption>>> GetSectionStudentsAsync(string userId, Guid sectionId)
        {
            var sender = await GetSenderAsync(userId);
            if (sender == null || !Permissions[sender.Role].ContainsKey(AudienceType.Students))
                return Result<List<NotificationTargetOption>>.Forbidden("You are not allowed to notify students.");

            var allowedSections = await GetAllowedSectionIdsAsync(sender);
            if (!allowedSections.Contains(sectionId))
                return Result<List<NotificationTargetOption>>.Forbidden("You are not allowed to notify this section.");

            var students = await GetStudentCandidatesAsync([sectionId], StudentDepartmentFilter(sender));
            return Result<List<NotificationTargetOption>>.Ok(ToOptions(students));
        }

        // INBOX

        public async Task<InboxDto> GetInboxAsync(string userId, bool unreadOnly, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var query = InboxQuery(userId);
            var unreadCount = await query.CountAsync(r => !r.IsRead);
            if (unreadOnly)
                query = query.Where(r => !r.IsRead);

            var total = await query.CountAsync();
            var rows = await query
                .OrderByDescending(r => r.Announcement!.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.AnnouncementId,
                    r.Announcement!.Title,
                    r.Announcement.Body,
                    r.Announcement.Priority,
                    SenderName = r.Announcement.Sender!.FullName,
                    r.Announcement.SenderRole,
                    r.Announcement.CreatedAt,
                    r.IsRead,
                    r.ReadAt
                })
                .ToListAsync();

            return new InboxDto
            {
                Items = rows.Select(r => new AnnouncementDto
                {
                    Id = r.AnnouncementId,
                    Title = r.Title,
                    Message = r.Body,
                    Priority = r.Priority.ToString(),
                    SenderName = r.SenderName ?? string.Empty,
                    SenderRole = r.SenderRole,
                    CreatedAt = AsUtc(r.CreatedAt),
                    IsRead = r.IsRead,
                    ReadAt = AsUtc(r.ReadAt)
                }).ToList(),
                Total = total,
                UnreadCount = unreadCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<int> GetUnreadCountAsync(string userId)
        {
            return InboxQuery(userId).CountAsync(r => !r.IsRead);
        }

        public async Task<Result<bool>> MarkAsReadAsync(string userId, Guid announcementId)
        {
            var updated = await _context.AnnouncementRecipients
                .Where(r => r.UserId == userId && r.AnnouncementId == announcementId && !r.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.IsRead, true)
                    .SetProperty(r => r.ReadAt, DateTime.UtcNow));

            if (updated > 0)
                return Result<bool>.Ok(true);

            // Already read is fine; only an unknown or someone else's notification is an error
            var exists = await _context.AnnouncementRecipients
                .AnyAsync(r => r.UserId == userId && r.AnnouncementId == announcementId);
            return exists ? Result<bool>.Ok(true) : Result<bool>.NotFound("Notification not found.");
        }

        public Task<int> MarkAllAsReadAsync(string userId, IReadOnlyCollection<Guid>? announcementIds)
        {
            var query = _context.AnnouncementRecipients.Where(r => r.UserId == userId && !r.IsRead);
            if (announcementIds is { Count: > 0 })
                query = query.Where(r => announcementIds.Contains(r.AnnouncementId));

            return query.ExecuteUpdateAsync(s => s
                .SetProperty(r => r.IsRead, true)
                .SetProperty(r => r.ReadAt, DateTime.UtcNow));
        }

        // SENDER / ADMIN STATUS

        public async Task<List<AnnouncementStatusDto>> GetSentAsync(string userId, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var query = _context.Announcements.AsNoTracking();
            if (!await IsAdminAsync(userId))
                query = query.Where(a => a.SenderId == userId);

            var ids = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => a.Id)
                .ToListAsync();

            return await GetStatusesAsync(ids);
        }

        public async Task<Result<AnnouncementStatusDto>> GetStatusAsync(string userId, Guid announcementId)
        {
            var access = await CheckSenderAccessAsync(userId, announcementId);
            if (!access.Success)
                return access;

            return Result<AnnouncementStatusDto>.Ok((await GetStatusesAsync([announcementId])).Single());
        }

        public async Task<Result<AnnouncementStatusDto>> RetryAsync(string userId, Guid announcementId)
        {
            var access = await CheckSenderAccessAsync(userId, announcementId);
            if (!access.Success)
                return access;

            if (access.Value!.Status != NotificationStatus.Failed.ToString())
                return Result<AnnouncementStatusDto>.Failure("Only failed notifications can be retried.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.AnnouncementRecipients
                .Where(r => r.AnnouncementId == announcementId && r.DeliveryStatus == DeliveryStatus.Failed)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.DeliveryStatus, DeliveryStatus.Pending));
            await _context.Announcements
                .Where(a => a.Id == announcementId && a.Status == NotificationStatus.Failed)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.Status, NotificationStatus.Queued)
                    .SetProperty(a => a.Attempts, 0)
                    .SetProperty(a => a.NextAttemptAt, (DateTime?)null)
                    .SetProperty(a => a.LastError, (string?)null));
            await transaction.CommitAsync();

            _queueSignal.Notify();
            _logger.LogInformation("Notification {NotificationId} re-queued by {UserId}", announcementId, userId);

            return Result<AnnouncementStatusDto>.Ok((await GetStatusesAsync([announcementId])).Single());
        }

        // HELPERS

        /// <summary>Roles come from the database rather than the token, so a demotion applies immediately.</summary>
        private async Task<Sender?> GetSenderAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Contains("Admin") ? "Admin"
                : roles.Contains("HOD") ? "HOD"
                : roles.Contains("Teacher") ? "Teacher"
                : null;
            if (role == null)
                return null;

            if (role == "Admin")
                return new Sender(user, role, null);

            // HODs and Teachers need an active teacher profile; HODs also need a department to scope by
            var teacher = await _context.Teachers
                .AsNoTracking()
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.UserId == userId && t.IsActive);
            if (teacher == null || (role == "HOD" && teacher.DepartmentId == null))
                return null;

            return new Sender(user, role, teacher);
        }

        /// <summary>HODs only reach students of their own department; teachers reach everyone in their sections.</summary>
        private static Guid? StudentDepartmentFilter(Sender sender) => sender.Role == "HOD" ? sender.DepartmentId : null;

        private async Task<bool> IsAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && await _userManager.IsInRoleAsync(user, "Admin");
        }

        private async Task<List<Candidate>> GetCandidatesAsync(Sender sender, AudienceType audience, Guid? sectionId)
        {
            return audience switch
            {
                AudienceType.HODs => await GetTeacherCandidatesAsync(hods: true, departmentId: null),
                AudienceType.Teachers => await GetTeacherCandidatesAsync(hods: false, sender.Role == "HOD" ? sender.DepartmentId : null),
                AudienceType.Students => await GetStudentCandidatesAsync(
                    sectionId.HasValue ? [sectionId.Value] : await GetAllowedSectionIdsAsync(sender),
                    StudentDepartmentFilter(sender)),
                _ => new List<Candidate>()
            };
        }

        /// <summary>Active teachers split by HOD role; HOD-sent notifications stay within the HOD's department.</summary>
        private async Task<List<Candidate>> GetTeacherCandidatesAsync(bool hods, Guid? departmentId)
        {
            var hodUserIds = (await _userManager.GetUsersInRoleAsync("HOD")).Select(u => u.Id).ToHashSet();

            var query = _context.Teachers.AsNoTracking().Where(t => t.IsActive);
            if (departmentId != null)
                query = query.Where(t => t.DepartmentId == departmentId);

            var teachers = await query
                .Select(t => new Candidate(t.Teacher_Id, t.UserId, t.User.FullName, t.Department != null ? t.Department.Name : null))
                .ToListAsync();

            return teachers
                .Where(t => hodUserIds.Contains(t.UserId) == hods)
                .OrderBy(t => t.Name)
                .ToList();
        }

        /// <summary>
        /// Teachers: sections they are actively assigned to.
        /// HODs: active sections that have actively enrolled students of their department.
        /// </summary>
        private async Task<List<Guid>> GetAllowedSectionIdsAsync(Sender sender)
        {
            if (sender.Role == "HOD")
            {
                var departmentId = sender.DepartmentId;
                return await _context.StudentEnrollments
                    .AsNoTracking()
                    .Where(e => e.IsActive
                        && e.Student != null && e.Student.DepartmentId == departmentId
                        && e.EnrolledSection != null && e.EnrolledSection.IsActive)
                    .Select(e => e.SectionId)
                    .Distinct()
                    .ToListAsync();
            }

            if (sender.Role == "Teacher" && sender.Teacher != null)
            {
                var teacherId = sender.Teacher.Teacher_Id;
                return await _context.TeacherSectionCourses
                    .AsNoTracking()
                    .Where(a => a.IsActive && a.TeacherId == teacherId && a.Section != null && a.Section.IsActive)
                    .Select(a => a.SectionId)
                    .Distinct()
                    .ToListAsync();
            }

            return new List<Guid>();
        }

        private async Task<List<Candidate>> GetStudentCandidatesAsync(List<Guid> sectionIds, Guid? departmentId)
        {
            if (sectionIds.Count == 0)
                return new List<Candidate>();

            var enrollments = _context.StudentEnrollments
                .AsNoTracking()
                .Where(e => e.IsActive && sectionIds.Contains(e.SectionId) && e.Student != null);
            if (departmentId != null)
                enrollments = enrollments.Where(e => e.Student!.DepartmentId == departmentId);

            var students = await enrollments
                .Select(e => new Candidate(
                    e.StudentId,
                    e.Student!.UserId,
                    e.Student.User.FullName,
                    e.EnrolledSection!.IntermediateClass + " - " + e.EnrolledSection.SectionName))
                .ToListAsync();

            return students
                .DistinctBy(s => s.Id)
                .OrderBy(s => s.Name)
                .ToList();
        }

        private IQueryable<AnnouncementRecipient> InboxQuery(string userId)
        {
            var now = DateTime.UtcNow;
            return _context.AnnouncementRecipients
                .AsNoTracking()
                .Where(r => r.UserId == userId && (r.Announcement!.ExpiresAt == null || r.Announcement.ExpiresAt > now));
        }

        /// <summary>Only the sender or an administrator may view or retry a notification's delivery status.</summary>
        private async Task<Result<AnnouncementStatusDto>> CheckSenderAccessAsync(string userId, Guid announcementId)
        {
            var senderId = await _context.Announcements
                .Where(a => a.Id == announcementId)
                .Select(a => a.SenderId)
                .FirstOrDefaultAsync();

            if (senderId == null || (senderId != userId && !await IsAdminAsync(userId)))
                return Result<AnnouncementStatusDto>.NotFound("Notification not found.");

            return Result<AnnouncementStatusDto>.Ok((await GetStatusesAsync([announcementId])).Single());
        }

        private async Task<List<AnnouncementStatusDto>> GetStatusesAsync(List<Guid> ids)
        {
            var rows = await _context.Announcements
                .AsNoTracking()
                .Where(a => ids.Contains(a.Id))
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Audience,
                    a.Scope,
                    Section = a.Section == null ? null : a.Section.IntermediateClass + " - " + a.Section.SectionName,
                    SenderName = a.Sender!.FullName,
                    a.SenderRole,
                    a.CreatedAt,
                    a.Status,
                    a.Attempts,
                    a.NextAttemptAt,
                    a.ProcessedAt,
                    a.LastError,
                    a.RecipientCount,
                    Pending = a.Recipients.Count(r => r.DeliveryStatus == DeliveryStatus.Pending),
                    Attempted = a.Recipients.Count(r => r.DeliveryStatus == DeliveryStatus.Attempted),
                    Acknowledged = a.Recipients.Count(r => r.DeliveryStatus == DeliveryStatus.Acknowledged),
                    Failed = a.Recipients.Count(r => r.DeliveryStatus == DeliveryStatus.Failed),
                    ReadCount = a.Recipients.Count(r => r.IsRead)
                })
                .ToListAsync();

            return rows
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new AnnouncementStatusDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Audience = r.Audience.ToString(),
                    Scope = r.Scope.ToString(),
                    Section = r.Section,
                    SenderName = r.SenderName ?? string.Empty,
                    SenderRole = r.SenderRole,
                    CreatedAt = AsUtc(r.CreatedAt),
                    Status = r.Status.ToString(),
                    Attempts = r.Attempts,
                    NextAttemptAt = AsUtc(r.NextAttemptAt),
                    ProcessedAt = AsUtc(r.ProcessedAt),
                    LastError = r.LastError,
                    RecipientCount = r.RecipientCount,
                    Pending = r.Pending,
                    Attempted = r.Attempted,
                    Acknowledged = r.Acknowledged,
                    Failed = r.Failed,
                    ReadCount = r.ReadCount
                })
                .ToList();
        }

        private static List<NotificationTargetOption> ToOptions(IEnumerable<Candidate> candidates)
        {
            return candidates
                .Select(c => new NotificationTargetOption { Id = c.Id, Label = c.Name, Detail = c.Detail })
                .ToList();
        }

        // Timestamps are stored as UTC but come back from SQL Server as Unspecified; mark them so JSON carries a "Z"
        private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

        private static DateTime? AsUtc(DateTime? value) => value.HasValue ? AsUtc(value.Value) : null;

        private static bool TryParseEnum<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
        {
            // Reject numeric strings so only the documented names are accepted
            return Enum.TryParse(value, ignoreCase: true, out result)
                && Enum.IsDefined(result)
                && !int.TryParse(value, out _);
        }
    }
}
