namespace AdvancedAspNetCoreLab.Background;

public sealed class QueuedBackgroundService : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<QueuedBackgroundService> _logger;

    public QueuedBackgroundService(
        IBackgroundTaskQueue queue,
        ILogger<QueuedBackgroundService> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            Func<CancellationToken, ValueTask> workItem =
                await _queue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown cancellation should not be logged as a failure.
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "A queued background task failed");
            }
        }

        _logger.LogInformation("Queued background service stopped");
    }
}
