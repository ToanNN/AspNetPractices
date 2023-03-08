namespace CleaningWorker.UsingScopeServicesWithHostedServices;

public class ScopedBackgroundService:BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScopedBackgroundService> _logger;

    public ScopedBackgroundService(IServiceProvider serviceProvider,
        ILogger<ScopedBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            $"{nameof(ScopedBackgroundService)} is running.");

        await DoWorkAsync(stoppingToken);

    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            $"{nameof(ScopedBackgroundService)} is working.");

        //get an instance of scope service in singleton context
        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            IScopedProcessingService serviceInstance =
                scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();
            await serviceInstance.DoWorkAsync(stoppingToken);
        }
    }
    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            $"{nameof(ScopedBackgroundService)} is stopping.");

        await base.StopAsync(stoppingToken);
    }
}