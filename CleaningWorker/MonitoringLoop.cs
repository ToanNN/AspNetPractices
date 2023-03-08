namespace CleaningWorker;

public class MonitoringLoop
{
    private readonly CancellationToken _cancellationToken;
    private readonly ILogger<MonitoringLoop> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public MonitoringLoop(
        IBackgroundTaskQueue taskQueue,
        ILogger<MonitoringLoop> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        //This cancellation token is called when the application is stopping
        _cancellationToken = applicationLifetime.ApplicationStopping;
    }

    public void StartMonitoring()
    {
        _logger.LogInformation($"{nameof(MonitorAsync)} loop is starting.");
        // Run a console user input loop in a background thread
        new Thread(async () => await MonitorAsync()).Start();
    }

    private async ValueTask MonitorAsync()
    {
        //Run until the application is stopped
        while (!_cancellationToken.IsCancellationRequested)
        {
            var keyStroke = Console.ReadKey();
            if (keyStroke.Key == ConsoleKey.W)
            {
                await _taskQueue.QueueBackgroundWorkItemAsync(BuildWorkItemAsync);
            }
        }
    }

    private async ValueTask BuildWorkItemAsync(CancellationToken cancellationToken)
    {
        // Simulate three 5 - second tasks to complete
        // for each enqueued work item

        int delayLoop = 0;
        var guid = Guid.NewGuid();

        _logger.LogInformation("Queued work item {Guid} is starting.", guid);

        while (!cancellationToken.IsCancellationRequested && delayLoop < 3)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if the Delay is cancelled
            }

            ++delayLoop;

            _logger.LogInformation("Queued work item {Guid} is running. {DelayLoop}/3", guid, delayLoop);
        }

        string format = delayLoop switch
        {
            3 => "Queued Background Task {Guid} is complete.",
            _ => "Queued Background Task {Guid} was cancelled."
        };

        _logger.LogInformation(format, guid);
    }
}