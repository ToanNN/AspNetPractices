using CleaningWorker;
using CleaningWorker.UsingScopeServicesWithHostedServices;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<MonitoringLoop>();
        services.AddHostedService<QueueHostedService>();
        services.AddSingleton<IBackgroundTaskQueue>(_ =>
        {
            if (!int.TryParse(context.Configuration["QueueCapacity"], out var queueCapacity))
            {
                queueCapacity = 100;
            }

            return new DefaultBackgroundTaskQueue(queueCapacity);
        });

        // This will configure ScopedBackgroundService as Singleton so you will need to get a scope from service provider to resolve scoped instances
        services.AddHostedService<ScopedBackgroundService>();
        services.AddScoped<IScopedProcessingService, DefaultScopedProcessingService>();
    })
    .Build();

//Start background thread to monitor the key strokes
var monitor = host.Services.GetRequiredService<MonitoringLoop>();
monitor.StartMonitoring();

host.Run();
