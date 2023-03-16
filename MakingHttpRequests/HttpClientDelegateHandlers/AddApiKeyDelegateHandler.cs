namespace MakingHttpRequests.HttpClientDelegateHandlers;

//IHttpClientFactory creates a separate DI scope for each handler, which can lead to surprising behavior when a handler consumes a scoped service.
public class AddApiKeyDelegateHandler: DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        const string headerName = "X-KEY-SCORE";
        if (!request.Headers.Contains(headerName))
        {
            request.Headers.Add(headerName, "198277277");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}