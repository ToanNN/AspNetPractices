using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

internal class Program
{
    private static void Main(string[] args)
    {
        var logger = LogManager.GetCurrentClassLogger();

        try
        {
            //Configure the app
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            using var serviceProvider = new ServiceCollection()
                .AddTransient<Runner>()
                .AddLogging(logBuilder =>
                {
                    logBuilder.ClearProviders();
                    logBuilder.SetMinimumLevel(LogLevel.Trace);
                    logBuilder.AddNLog(config);
                }).BuildServiceProvider();

            //Get the class and perform the action
            var runner = serviceProvider.GetRequiredService<Runner>();
            runner.DoAction("coding");

            Console.WriteLine("Finished");
        }
        catch (Exception exception)
        {
            logger.Error(exception, "Unexpected exception");

            throw;
        }
        finally
        {
            LogManager.Shutdown();
        }

        Console.ReadLine();
    }
}