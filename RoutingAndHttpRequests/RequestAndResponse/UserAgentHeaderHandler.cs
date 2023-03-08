using Microsoft.Net.Http.Headers;

namespace RoutingAndHttpRequests.RequestAndResponse;

public class UserAgentHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<UserAgentHeaderHandler> _logger;

    public UserAgentHeaderHandler(IHttpContextAccessor contextAccessor, ILogger<UserAgentHeaderHandler> logger)
    {
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var contextRequest = _contextAccessor.HttpContext?.Request;
        string? userAgent = contextRequest?.Headers["user-agent"].ToString();

        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = "Unknown";
        }

        request.Headers.Add(HeaderNames.UserAgent, userAgent);
        _logger.LogInformation($"User-Agent: {userAgent}");

        return await base.SendAsync(request, cancellationToken);
    }
}