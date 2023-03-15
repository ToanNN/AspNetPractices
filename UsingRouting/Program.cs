using Microsoft.AspNetCore.Authentication.Negotiate;
using UsingRouting.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var timerCount = 0;
app.Use(async (context, next) =>
{
    using (new AutoStopwatch(logger, $"Time {++timerCount}"))
    {
        await next(context);
    }
});

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    using (new AutoStopwatch(logger, $"Time {++timerCount}"))
    {
        await next(context);
    }
});

app.UseAuthorization();
app.Use(async (context, next) =>
{
    using (new AutoStopwatch(logger, $"Time {++timerCount}"))
    {
        await next(context);
    }
});

app.MapControllers();

app.Use(async (context, next) =>
{
    var currentEndpoint = context.GetEndpoint();
    if (currentEndpoint is null)
    {
        await next(context);
        return;
    }

    Console.WriteLine($"Matched endpoint is {currentEndpoint.DisplayName}");

    if (currentEndpoint is RouteEndpoint routeEndpoint)
    {
        Console.WriteLine($"    - Route Pattern: {routeEndpoint.RoutePattern}");
    }

    foreach (var endpointMetadata in currentEndpoint.Metadata)
    {
        Console.WriteLine($"    - Metadata: {endpointMetadata}");
    }

    await next(context);
});

app.MapGet("/endpoint", () => "Inspecto");
app.Run();