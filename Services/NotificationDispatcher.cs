using System.Linq.Expressions;
using Auth.Data;
using Auth.Hubs;
using Auth.Model.DTOs.Announcements;
using Auth.Model.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Auth.Services
{
    /// <summary>
    /// Background worker that turns Queued notifications into SignalR pushes.
    ///
    /// Jobs live in the Announcements table (the outbox), so nothing is lost on restart. A job is claimed with
    /// a conditional UPDATE, which only one worker can win, and held by a lease; if the worker dies the lease
    /// expires and the job is reclaimed. Recipients are pushed in batches and marked Attempted afterwards, so a
    /// retry only re-sends batches that were not recorded. A crash between a push and its status update can
    /// re-send that one batch; clients de-duplicate by notification id.
    /// </summary>
    public sealed class NotificationDispatcher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly NotificationQueueSignal _signal;
        private readonly NotificationDispatchOptions _options;
        private readonly ILogger<NotificationDispatcher> _logger;
        private readonly string _workerId = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

        public NotificationDispatcher(
            IServiceScopeFactory scopeFactory,
            IHubContext<NotificationHub> hubContext,
            NotificationQueueSignal signal,
            IOptions<NotificationDispatchOptions> options,
            ILogger<NotificationDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _signal = signal;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification dispatcher {WorkerId} started", _workerId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Drain everything claimable, then sleep until signalled or the poll interval passes
                    while (!stoppingToken.IsCancellationRequested && await TryProcessNextAsync(stoppingToken))
                    {
                    }

                    await _signal.WaitAsync(_options.PollInterval, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // e.g. database unavailable; back off and try again rather than stopping the host
                    _logger.LogError(ex, "Notification dispatcher loop failed; retrying in {Delay}", _options.PollInterval);
                    try
                    {
                        await Task.Delay(_options.PollInterval, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }

            _logger.LogInformation("Notification dispatcher {WorkerId} stopped", _workerId);
        }

        private async Task<bool> TryProcessNextAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var jobId = await ClaimNextAsync(db, stoppingToken);
            if (jobId == null)
                return false;

            await ProcessAsync(db, jobId.Value, stoppingToken);
            return true;
        }

        private static Expression<Func<Announcement, bool>> IsClaimable(DateTime now) =>
            a => (a.Status == NotificationStatus.Queued && (a.NextAttemptAt == null || a.NextAttemptAt <= now))
                 || (a.Status == NotificationStatus.Processing && a.LockedUntil < now);

        private async Task<Guid?> ClaimNextAsync(ApplicationDbContext db, CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            var candidates = await db.Announcements
                .AsNoTracking()
                .Where(IsClaimable(now))
                .OrderBy(a => a.CreatedAt)
                .Select(a => new { a.Id, a.Status })
                .Take(10)
                .ToListAsync(stoppingToken);

            var leaseUntil = now.Add(_options.LeaseDuration);
            foreach (var candidate in candidates)
            {
                // Single conditional UPDATE: if another worker claimed it first, no row matches
                var claimed = await db.Announcements
                    .Where(a => a.Id == candidate.Id)
                    .Where(IsClaimable(now))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(a => a.Status, NotificationStatus.Processing)
                        .SetProperty(a => a.LockedUntil, leaseUntil)
                        .SetProperty(a => a.LockedBy, _workerId)
                        .SetProperty(a => a.Attempts, a => a.Attempts + 1), stoppingToken);

                if (claimed == 1)
                {
                    if (candidate.Status == NotificationStatus.Processing)
                        _logger.LogWarning("Reclaimed notification {NotificationId} after its lease expired", candidate.Id);
                    return candidate.Id;
                }
            }

            return null;
        }

        private async Task ProcessAsync(ApplicationDbContext db, Guid jobId, CancellationToken stoppingToken)
        {
            var job = await db.Announcements
                .AsNoTracking()
                .Where(a => a.Id == jobId)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Body,
                    a.Priority,
                    a.SenderRole,
                    SenderName = a.Sender!.FullName,
                    a.CreatedAt,
                    a.Attempts
                })
                .FirstAsync(stoppingToken);

            // Same payload for every recipient: no recipient lists or other personal data
            var payload = new AnnouncementDto
            {
                Id = job.Id,
                Title = job.Title,
                Message = job.Body,
                Priority = job.Priority.ToString(),
                SenderName = job.SenderName ?? string.Empty,
                SenderRole = job.SenderRole,
                CreatedAt = DateTime.SpecifyKind(job.CreatedAt, DateTimeKind.Utc),
                IsRead = false
            };

            _logger.LogInformation("Processing notification {NotificationId} (attempt {Attempt}/{MaxAttempts})", jobId, job.Attempts, _options.MaxAttempts);

            var pushed = 0;
            try
            {
                while (true)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    var batch = await db.AnnouncementRecipients
                        .AsNoTracking()
                        .Where(r => r.AnnouncementId == jobId && r.DeliveryStatus == DeliveryStatus.Pending)
                        .OrderBy(r => r.Id)
                        .Select(r => new { r.Id, r.UserId })
                        .Take(_options.BatchSize)
                        .ToListAsync(stoppingToken);

                    if (batch.Count == 0)
                        break;

                    var recipientIds = batch.Select(r => r.Id).ToList();
                    var attemptedAt = DateTime.UtcNow;

                    try
                    {
                        // Reaches every open connection of each user (tabs, devices); offline users get it from the inbox
                        await _hubContext.Clients
                            .Users(batch.Select(r => r.UserId).ToList())
                            .SendAsync(NotificationHub.ReceiveNotification, payload, stoppingToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        var batchError = Truncate(ex.Message, 500);
                        await db.AnnouncementRecipients
                            .Where(r => recipientIds.Contains(r.Id))
                            .ExecuteUpdateAsync(s => s
                                .SetProperty(r => r.DeliveryAttempts, r => r.DeliveryAttempts + 1)
                                .SetProperty(r => r.LastAttemptAt, attemptedAt)
                                .SetProperty(r => r.LastError, batchError), CancellationToken.None);
                        throw;
                    }

                    // Keep Acknowledged if the client confirmed before this update ran
                    await db.AnnouncementRecipients
                        .Where(r => recipientIds.Contains(r.Id))
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(r => r.DeliveryStatus, r => r.DeliveryStatus == DeliveryStatus.Pending ? DeliveryStatus.Attempted : r.DeliveryStatus)
                            .SetProperty(r => r.DeliveryAttempts, r => r.DeliveryAttempts + 1)
                            .SetProperty(r => r.LastAttemptAt, attemptedAt)
                            .SetProperty(r => r.LastError, (string?)null), CancellationToken.None);

                    pushed += batch.Count;

                    // Renew the lease; if it was lost another worker owns the job now
                    var leaseUntil = DateTime.UtcNow.Add(_options.LeaseDuration);
                    var renewed = await db.Announcements
                        .Where(a => a.Id == jobId && a.LockedBy == _workerId)
                        .ExecuteUpdateAsync(s => s.SetProperty(a => a.LockedUntil, leaseUntil), stoppingToken);
                    if (renewed == 0)
                    {
                        _logger.LogWarning("Lost the lease on notification {NotificationId} after {Pushed} recipients; stopping", jobId, pushed);
                        return;
                    }
                }

                var processedAt = DateTime.UtcNow;
                await db.Announcements
                    .Where(a => a.Id == jobId && a.LockedBy == _workerId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(a => a.Status, NotificationStatus.Processed)
                        .SetProperty(a => a.ProcessedAt, processedAt)
                        .SetProperty(a => a.NextAttemptAt, (DateTime?)null)
                        .SetProperty(a => a.LockedUntil, (DateTime?)null)
                        .SetProperty(a => a.LockedBy, (string?)null)
                        .SetProperty(a => a.LastError, (string?)null), CancellationToken.None);

                _logger.LogInformation("Notification {NotificationId} processed; pushed to {Pushed} recipients", jobId, pushed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                await ReleaseForShutdownAsync(db, jobId);
                throw;
            }
            catch (Exception ex)
            {
                await HandleFailureAsync(db, jobId, job.Attempts, ex);
            }
        }

        /// <summary>Hands the job back on shutdown so it resumes on the next start without using up an attempt.</summary>
        private async Task ReleaseForShutdownAsync(ApplicationDbContext db, Guid jobId)
        {
            try
            {
                await db.Announcements
                    .Where(a => a.Id == jobId && a.LockedBy == _workerId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(a => a.Status, NotificationStatus.Queued)
                        .SetProperty(a => a.Attempts, a => a.Attempts - 1)
                        .SetProperty(a => a.LockedUntil, (DateTime?)null)
                        .SetProperty(a => a.LockedBy, (string?)null), CancellationToken.None);
                _logger.LogInformation("Released notification {NotificationId} for shutdown", jobId);
            }
            catch (Exception ex)
            {
                // The lease will expire and another start will reclaim it
                _logger.LogWarning(ex, "Could not release notification {NotificationId} on shutdown", jobId);
            }
        }

        private async Task HandleFailureAsync(ApplicationDbContext db, Guid jobId, int attempts, Exception error)
        {
            var message = Truncate(error.Message, 2000);
            try
            {
                if (attempts >= _options.MaxAttempts)
                {
                    await db.Announcements
                        .Where(a => a.Id == jobId && a.LockedBy == _workerId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(a => a.Status, NotificationStatus.Failed)
                            .SetProperty(a => a.LastError, message)
                            .SetProperty(a => a.NextAttemptAt, (DateTime?)null)
                            .SetProperty(a => a.LockedUntil, (DateTime?)null)
                            .SetProperty(a => a.LockedBy, (string?)null), CancellationToken.None);

                    var recipientError = Truncate($"Gave up after {attempts} attempts: {error.Message}", 500);
                    await db.AnnouncementRecipients
                        .Where(r => r.AnnouncementId == jobId && r.DeliveryStatus == DeliveryStatus.Pending)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(r => r.DeliveryStatus, DeliveryStatus.Failed)
                            .SetProperty(r => r.LastError, recipientError), CancellationToken.None);

                    _logger.LogError(error, "Notification {NotificationId} failed permanently after {Attempts} attempts", jobId, attempts);
                }
                else
                {
                    var delay = RetryDelay(attempts);
                    var nextAttemptAt = DateTime.UtcNow.Add(delay);
                    await db.Announcements
                        .Where(a => a.Id == jobId && a.LockedBy == _workerId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(a => a.Status, NotificationStatus.Queued)
                            .SetProperty(a => a.LastError, message)
                            .SetProperty(a => a.NextAttemptAt, nextAttemptAt)
                            .SetProperty(a => a.LockedUntil, (DateTime?)null)
                            .SetProperty(a => a.LockedBy, (string?)null), CancellationToken.None);

                    _logger.LogWarning(error, "Notification {NotificationId} attempt {Attempt} failed; retrying in {Delay}", jobId, attempts, delay);
                }
            }
            catch (Exception updateError)
            {
                // The lease will expire and the job will be reclaimed
                _logger.LogError(updateError, "Could not record failure for notification {NotificationId}", jobId);
            }
        }

        private TimeSpan RetryDelay(int attempts)
        {
            var factor = Math.Pow(2, Math.Max(0, attempts - 1));
            var delay = TimeSpan.FromTicks((long)Math.Min(_options.RetryBaseDelay.Ticks * factor, _options.RetryMaxDelay.Ticks));
            return delay;
        }

        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..maxLength];
    }
}
