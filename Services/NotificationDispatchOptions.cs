namespace Auth.Services
{
    /// <summary>Bound from the "Notifications" configuration section; defaults suit a single small server.</summary>
    public class NotificationDispatchOptions
    {
        public const string SectionName = "Notifications";

        /// <summary>Recipients pushed per SignalR call and per status update.</summary>
        public int BatchSize { get; set; } = 200;

        /// <summary>Claims of one notification before it is marked Failed.</summary>
        public int MaxAttempts { get; set; } = 5;

        public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>How long a worker owns a job; renewed after every batch.</summary>
        public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>Retry delay doubles per attempt from this value, capped at RetryMaxDelay.</summary>
        public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(15);

        public TimeSpan RetryMaxDelay { get; set; } = TimeSpan.FromMinutes(10);
    }
}
