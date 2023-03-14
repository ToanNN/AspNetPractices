using UsingOutputCaching.Middleware.ScopeServices;

namespace UsingOutputCaching.FactoryBasedMiddlewareActivation;

public class FactoryActivatedMiddleware:IMiddleware
{
    private readonly IMessageWriter _writer;

    public FactoryActivatedMiddleware(IMessageWriter writer)
    {
        _writer = writer;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var keyValue = context.Request.Query["key"];
        if (!string.IsNullOrEmpty(keyValue))
        {
            _writer.Write("Factory Based: " + keyValue);
        }
        return Task.CompletedTask;
    }
}