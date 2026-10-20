//using Auth.Data;
//using Auth.Model.DTOs.Announcements;
//using Auth.Model.Entities;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//namespace Auth.Services
//{
//    public class AnnouncementService : IAnnouncementService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly ILogger<AnnouncementService> _logger;

//        public AnnouncementService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AnnouncementService> logger)
//        {
//            _context = context;
//            _userManager = userManager;
//            _logger = logger;
//        }

//        //public async Task<Result<AnnouncementCreatedDto>> CreateAnnouncementAsync(string senderId, CreateAnnouncementDto dto)
//        {
//            try
//            {
//                // Validate sender
//                var sender = await _userManager.FindByIdAsync(senderId);
//                if (sender == null)
//                    return Result<AnnouncementCreatedDto>.Failure("Sender not found.");

//                var roles = await _userManager.GetRolesAsync(sender);

//                // Only HOD and Teacher can send
//                if (!roles.Contains("HOD") && !roles.Contains("Teacher"))
//                    return Result<AnnouncementCreatedDto>.Failure("Only HOD and Teacher roles can send announcements.");

//                string senderRole;
//                List<string> recipientUserIds = new();

//                // Determine sender role and get recipients
//                if (roles.Contains("HOD") && !dto.CourseId.HasValue)
//                {
//                    // HOD mode: send to teachers/students in their department
//                    senderRole = "HOD";
//                    var result = await GetHodRecipients(senderId, dto.Audience);
//                    if (!result.Success)
//                        return Result<AnnouncementCreatedDto>.Failure(result.Error);
//                    recipientUserIds = result.Value!;
//                }
//                else if (roles.Contains("Teacher") || (roles.Contains("HOD") && dto.CourseId.HasValue))
//                {
//                    // Teacher mode: send to students in courses they teach
//                    senderRole = "Teacher";

//                    if (!dto.CourseId.HasValue)
//                        return Result<AnnouncementCreatedDto>.Failure("CourseId is required for Teacher mode.");

//                    if (dto.Audience != AudienceType.Students)
//                        return Result<AnnouncementCreatedDto>.Failure("Teachers can only send to Students.");

//                    var result = await GetTeacherRecipients(senderId, dto.CourseId.Value, dto.SectionId);
//                    if (!result.Success)
//                        return Result<AnnouncementCreatedDto>.Failure(result.Error);
//                    recipientUserIds = result.Value!;
//                }
//                else
//                {
//                    return Result<AnnouncementCreatedDto>.Failure("Invalid role for sending announcements.");
//                }

//                // Exclude sender
//                recipientUserIds = recipientUserIds.Where(id => id != senderId).ToList();

//                // Distinct recipients
//                recipientUserIds = recipientUserIds.Distinct().ToList();

//                if (recipientUserIds.Count == 0)
//                    return Result<AnnouncementCreatedDto>.Failure("No recipients found.");

//                // Create announcement
//                var announcement = new Announcement
//                {
//                    Id = Guid.NewGuid(),
//                    Title = dto.Title,
//                    Body = dto.Body,
//                    Priority = dto.Priority,
//                    Audience = dto.Audience,
//                    SenderId = senderId,
//                    SenderRole = senderRole,
//                    CreatedAt = DateTime.UtcNow,
//                    ExpiresAt = dto.ExpiresAt
//                };

//                _context.Announcements.Add(announcement);

//                // Create recipients
//                foreach (var userId in recipientUserIds)
//                {
//                    _context.AnnouncementRecipients.Add(new AnnouncementRecipient
//                    {
//                        Id = Guid.NewGuid(),
//                        AnnouncementId = announcement.Id,
//                        UserId = userId,
//                        IsRead = false
//                    });
//                }

//                await _context.SaveChangesAsync();

//                return Result<AnnouncementCreatedDto>.Ok(new AnnouncementCreatedDto { Id = announcement.Id });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating announcement");
//                return Result<AnnouncementCreatedDto>.Failure("An error occurred while creating the announcement.");
//            }
//        }

//        private async Task<Result<List<string>>> GetHodRecipients(string hodUserId, AudienceType audience)
//        {
//            try
//            {
//                // Get HOD's teacher record and department
//                var hodTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == hodUserId);
//                if (hodTeacher == null)
//                    return Result<List<string>>.Failure("HOD profile not found.");

//                var department = hodTeacher.Department?.Trim();
//                if (string.IsNullOrEmpty(department))
//                    return Result<List<string>>.Failure("HOD department is not set.");

//                var recipients = new List<string>();

//                // Include teachers
//                if (audience == AudienceType.Teachers || audience == AudienceType.TeachersAndStudents)
//                {
//                    var teacherIds = await _context.Teachers
//                        .Where(t => t.Department != null && t.Department.Trim().Equals(department, StringComparison.OrdinalIgnoreCase))
//                        .Select(t => t.UserId)
//                        .ToListAsync();
//                    recipients.AddRange(teacherIds);
//                }

//                // Include students
//                if (audience == AudienceType.Students || audience == AudienceType.TeachersAndStudents)
//                {
//                    // Student is in department if enrolled in at least one section where a teacher in that department teaches
//                    var studentIds = await _context.StudentEnrollments
//                        .Where(se => se.IsActive)
//                        .Join(
//                            _context.TeacherSectionCourses.Where(tsc => tsc.IsActive),
//                            se => se.SectionId,
//                            tsc => tsc.SectionId,
//                            (se, tsc) => new { se.StudentId, tsc.TeacherId }
//                        )
//                        .Join(
//                            _context.Teachers,
//                            x => x.TeacherId,
//                            t => t.Teacher_Id,
//                            (x, t) => new { x.StudentId, t.Department }
//                        )
//                        .Where(x => x.Department != null && x.Department.Trim().Equals(department, StringComparison.OrdinalIgnoreCase))
//                        .Select(x => x.StudentId)
//                        .Distinct()
//                        .ToListAsync();

//                    // Get Student.UserId for each StudentId
//                    var studentUserIds = await _context.Students
//                        .Where(s => studentIds.Contains(s.StudentId))
//                        .Select(s => s.UserId)
//                        .ToListAsync();
//                    recipients.AddRange(studentUserIds);
//                }

//                return Result<List<string>>.Ok(recipients.Distinct().ToList());
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting HOD recipients");
//                return Result<List<string>>.Failure("Error retrieving HOD recipients.");
//            }
//        }

//        private async Task<Result<List<string>>> GetTeacherRecipients(string teacherUserId, Guid courseId, Guid? sectionId)
//        {
//            try
//            {
//                // Get teacher's assignments for the course
//                var teacherAssignments = await _context.TeacherSectionCourses
//                    .Where(tsc => tsc.Teacher != null && tsc.Teacher.UserId == teacherUserId && tsc.CourseId == courseId && tsc.IsActive)
//                    .ToListAsync();

//                if (teacherAssignments.Count == 0)
//                    return Result<List<string>>.Failure("Teacher does not teach the specified course.");

//                // Filter by section if provided
//                IEnumerable<Guid> relevantSectionIds = teacherAssignments.Select(ta => ta.SectionId);
//                if (sectionId.HasValue)
//                {
//                    if (!relevantSectionIds.Contains(sectionId.Value))
//                        return Result<List<string>>.Failure("Teacher does not teach in the specified section.");
//                    relevantSectionIds = new[] { sectionId.Value };
//                }

//                // Get students enrolled in those sections
//                var studentUserIds = await _context.StudentEnrollments
//                    .Where(se => relevantSectionIds.Contains(se.SectionId) && se.IsActive)
//                    .Join(
//                        _context.Students,
//                        se => se.StudentId,
//                        s => s.StudentId,
//                        (se, s) => s.UserId
//                    )
//                    .Distinct()
//                    .ToListAsync();

//                return Result<List<string>>.Ok(studentUserIds);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting teacher recipients");
//                return Result<List<string>>.Failure("Error retrieving teacher recipients.");
//            }
//        }

//        public async Task<(List<AnnouncementDto> Items, int Total)> GetInboxAsync(string userId, bool unreadOnly, int page, int pageSize)
//        {
//            try
//            {
//                var now = DateTime.UtcNow;

//                var query = _context.AnnouncementRecipients
//                    .Where(ar => ar.UserId == userId)
//                    .Join(
//                        _context.Announcements.Where(a => a.ExpiresAt == null || a.ExpiresAt > now),
//                        ar => ar.AnnouncementId,
//                        a => a.Id,
//                        (ar, a) => new { ar, a }
//                    );

//                if (unreadOnly)
//                    query = query.Where(x => !x.ar.IsRead);

//                var total = await query.CountAsync();

//                var items = await query
//                    .OrderByDescending(x => x.a.CreatedAt)
//                    .Skip((page - 1) * pageSize)
//                    .Take(pageSize)
//                    .Select(x => new AnnouncementDto
//                    {
//                        Id = x.a.Id,
//                        Title = x.a.Title,
//                        Body = x.a.Body,
//                        Priority = x.a.Priority,
//                        Audience = x.a.Audience,
//                        SenderRole = x.a.SenderRole,
//                        CreatedAt = x.a.CreatedAt,
//                        ExpiresAt = x.a.ExpiresAt,
//                        IsRead = x.ar.IsRead,
//                        ReadAt = x.ar.ReadAt
//                    })
//                    .ToListAsync();

//                return (items, total);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting inbox");
//                return (new List<AnnouncementDto>(), 0);
//            }
//        }

//        public async Task<int> GetUnreadCountAsync(string userId)
//        {
//            try
//            {
//                var now = DateTime.UtcNow;

//                return await _context.AnnouncementRecipients
//                    .Where(ar => ar.UserId == userId && !ar.IsRead)
//                    .Join(
//                        _context.Announcements.Where(a => a.ExpiresAt == null || a.ExpiresAt > now),
//                        ar => ar.AnnouncementId,
//                        a => a.Id,
//                        (ar, a) => ar
//                    )
//                    .CountAsync();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting unread count");
//                return 0;
//            }
//        }

//        public async Task<Result<object>> MarkAsReadAsync(string userId, Guid announcementId)
//        {
//            try
//            {
//                var recipient = await _context.AnnouncementRecipients
//                    .FirstOrDefaultAsync(ar => ar.UserId == userId && ar.AnnouncementId == announcementId);

//                if (recipient == null)
//                    return Result<object>.Failure("Announcement not found for this user.");

//                if (recipient.IsRead)
//                    return Result<object>.Failure("Already marked as read.");

//                recipient.IsRead = true;
//                recipient.ReadAt = DateTime.UtcNow;

//                await _context.SaveChangesAsync();

//                return Result<object>.Ok(new { });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error marking announcement as read");
//                return Result<object>.Failure("An error occurred.");
//            }
//        }
//    }
//}
