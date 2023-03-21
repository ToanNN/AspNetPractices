using System.Text.Json;
using System.Threading.Channels;

namespace UsingMinimalAPIs;

public class BackgroundQueue : BackgroundService
{
    private readonly ILogger<BackgroundQueue> _logger;
    private readonly Channel<ReadOnlyMemory<byte>> _queue;

    public BackgroundQueue(Channel<ReadOnlyMemory<byte>> queue, ILogger<BackgroundQueue> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var dataStream in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                var person = JsonSerializer.Deserialize<Person>(dataStream.Span);

                //do something with a person
                _logger.LogInformation($"{person.Name} is {person.Age} years and from {person.Country}");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }
    }
}

internal class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Country { get; set; } = string.Empty;
}