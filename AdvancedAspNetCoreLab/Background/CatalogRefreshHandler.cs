using AdvancedAspNetCoreLab.Configuration;
using Microsoft.Extensions.Options;

namespace AdvancedAspNetCoreLab.Background;

public sealed class CatalogRefreshHandler
{
    private readonly JobStore _jobs;
    private readonly IOptionsSnapshot<CoursePlatformOptions> _options;
    private readonly ILogger<CatalogRefreshHandler> _logger;

    public CatalogRefreshHandler(
        JobStore jobs,
        IOptionsSnapshot<CoursePlatformOptions> options,
        ILogger<CatalogRefreshHandler> logger)
    {
        _jobs = jobs;
        _options = options;
        _logger = logger;
    }

    public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken)
    {
        _jobs.MarkRunning(jobId);
        _logger.LogInformation(
            "Processing catalog refresh job {JobId} from queue {QueueName}",
            jobId,
            _options.Value.QueueName);

        try
        {
            // Replace this delay with real I/O such as an API or database call.
            await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
            _jobs.MarkCompleted(jobId);
            _logger.LogInformation("Catalog refresh job {JobId} completed", jobId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _jobs.MarkFailed(jobId, exception.Message);
            throw;
        }
    }
}
