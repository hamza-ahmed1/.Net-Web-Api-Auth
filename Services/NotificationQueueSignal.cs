using System.Threading.Channels;

namespace Auth.Services
{
    /// <summary>
    /// Wakes the dispatcher as soon as a notification is queued instead of waiting for the next poll.
    /// It is only a hint: the database is the queue, so a lost signal just means the next poll picks the job up.
    /// </summary>
    public sealed class NotificationQueueSignal
    {
        private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropWrite
        });

        public void Notify() => _channel.Writer.TryWrite(true);

        /// <summary>Waits until signalled or the timeout passes; throws only when the host is stopping.</summary>
        public async Task WaitAsync(TimeSpan timeout, CancellationToken stoppingToken)
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeoutSource.CancelAfter(timeout);
            try
            {
                await _channel.Reader.ReadAsync(timeoutSource.Token);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // Poll interval elapsed
            }
        }
    }
}
