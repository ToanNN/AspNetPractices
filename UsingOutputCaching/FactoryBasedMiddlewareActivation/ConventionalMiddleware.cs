using UsingOutputCaching.Middleware.ScopeServices;

namespace UsingOutputCaching.FactoryBasedMiddlewareActivation;

public class ConventionalMiddleware
{
    private readonly RequestDelegate _next;

    public ConventionalMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Inject scoped service to InvokeAsync
    public async Task InvokeAsync(HttpContext context, IMessageWriter messageWriter)
    {
        var keyValue = context.Request.Query["key"];

        if (!string.IsNullOrWhiteSpace(keyValue))
        {
            messageWriter.Write("Conventional " + keyValue);
        }

        await _next(context);
    }
}