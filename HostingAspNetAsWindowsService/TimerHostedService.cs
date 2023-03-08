namespace HostingAspNetAsWindowsService;

public sealed class TimerHostedService : IHostedService, IAsyncDisposable
{
    private readonly ILogger<TimerHostedService> _logger;
    private int _executionCount;
    private Timer? _timer;

    public TimerHostedService(ILogger<TimerHostedService> logger)
    {
        _logger = logger;
    }

    public async ValueTask DisposeAsync()
    {
        if (_timer is IAsyncDisposable timer)
        {
            await timer.DisposeAsync();
        }

        _timer = null;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Service} is running.", nameof(TimerHostedService));
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "{Service} is stopping.", nameof(TimerHostedService));

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        int count = Interlocked.Increment(ref _executionCount);
        _logger.LogInformation("{Service} is working, execution count: {Count: #,0}", nameof(TimerHostedService),
            count);
    }
}