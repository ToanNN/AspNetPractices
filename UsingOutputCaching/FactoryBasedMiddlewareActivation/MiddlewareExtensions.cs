namespace UsingOutputCaching.FactoryBasedMiddlewareActivation;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseConventionalMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ConventionalMiddleware>();
    }

    public static IApplicationBuilder UseFactoryBasedMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FactoryActivatedMiddleware>();
    }
}