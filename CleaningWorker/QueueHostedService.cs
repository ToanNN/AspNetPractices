namespace CleaningWorker;

public class QueueHostedService : BackgroundService
{
    private readonly ILogger<QueueHostedService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public QueueHostedService(IBackgroundTaskQueue taskQueue, ILogger<QueueHostedService> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(QueueHostedService)} is running.{Environment.NewLine}" +
                               $"{Environment.NewLine}Tap W to add a work item to the " +
                               $"background queue.{Environment.NewLine}");

        return ProcessTaskQueueAsync(stoppingToken);
    }

    private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);
                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                //cancelled
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error occurred executing task work item.");
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(QueueHostedService)} is stopping.");

        await base.StopAsync(stoppingToken);
    }
}