using System.Net.Http;
using System.Text.Json;

namespace RoutingAndHttpRequests.RequestAndResponse;

public class PeriodicBranchLoggerService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PeriodicBranchLoggerService> _logger;
    private readonly PeriodicTimer _timer;

    public PeriodicBranchLoggerService(IHttpClientFactory httpClientFactory,
        ILogger<PeriodicBranchLoggerService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                // Cancel sending the request to sync branches if it takes too long
                // rather than miss sending the next request scheduled 30 seconds from now.
                // Having a single loop prevents this service from sending an unbounded
                // number of requests simultaneously.

                //Create a new cancellation token source that will be cancelled when stoppingToken is cancelled
                using var syncTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                syncTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

                var httpClient = _httpClientFactory.CreateClient("GitHub");
                var response = await httpClient.GetAsync("repos/dotnet/AspNetCore.Docs/branches",
                    stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    await using var contentStream =
                        await response.Content.ReadAsStreamAsync(stoppingToken);

                    // Sync the response with preferred datastore.
                    var responses = await JsonSerializer.DeserializeAsync<
                        IEnumerable<GitHubBranch>>(contentStream, cancellationToken: stoppingToken);

                    _logger.LogInformation(
                        $"Branch sync successful! Response: {JsonSerializer.Serialize(responses)}");
                }
                else
                {
                    _logger.LogError(1, $"Branch sync failed! HTTP status code: {response.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(1, exception, "Branch sync failed!");
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        return base.StopAsync(cancellationToken);
    }
}