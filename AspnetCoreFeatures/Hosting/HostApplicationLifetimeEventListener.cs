namespace AspnetCoreFeatures.Hosting;

public class HostApplicationLifetimeEventListener: IHostedService
{

    private readonly IHostApplicationLifetime _hostLifetime;
    private readonly IHostEnvironment _hostEnvironment;

    public HostApplicationLifetimeEventListener(IHostApplicationLifetime hostLifetime, IHostEnvironment hostEnvironment)
    {
        _hostLifetime = hostLifetime;
        _hostEnvironment = hostEnvironment;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _hostLifetime.ApplicationStarted.Register(OnStarted);
        _hostLifetime.ApplicationStopping.Register(OnStopping);
        _hostLifetime.ApplicationStopped.Register(OnStopped);

        return Task.CompletedTask;
    }

    private void OnStopped()
    {
        Console.WriteLine($"{_hostEnvironment.ApplicationName} stopped");
    }

    private void OnStopping()
    {
        Console.WriteLine($"{_hostEnvironment.ApplicationName} is stopping");
    }

    private void OnStarted()
    {
        Console.WriteLine($"{_hostEnvironment.ApplicationName} Started. EnvironmentName {_hostEnvironment.EnvironmentName}, ContentRootPath {_hostEnvironment.ContentRootPath}");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}