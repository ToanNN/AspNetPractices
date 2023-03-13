using System.Globalization;
using UsingOutputCaching.Middleware.ScopeServices;

namespace UsingOutputCaching.Middleware;

// The middleware class must include:
//A public constructor with a parameter of type RequestDelegate.
//    A public method named Invoke or InvokeAsync.This method must:
//Return a Task.
//    Accept a first parameter of type HttpContext.

//Typically an extension method is created to expose the middleware

//Middleware components can resolve their dependencies from dependency injection (DI) through constructor parameters

//Middleware is constructed at app startup and therefore has application life time. This means the instances injected into a middleware
// constructor will live through the life of the app
//Scoped lifetime services used by middleware constructors aren't shared with other dependency-injected types during each request
// To shared scoped services with other types, inject them into InvokeAsync() method
public class RequestCultureMiddleware
{
    private readonly RequestDelegate _next;

    public RequestCultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMessageWriter messageWriter)
    {
        var cultureQuery = context.Request.Query["culture"];
        if (!string.IsNullOrWhiteSpace(cultureQuery))
        {
            var culture = new CultureInfo(cultureQuery);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        await _next(context);
    }
}

public static class RequestCultureMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestCulture(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestCultureMiddleware>();
    }
}