using RateLimitingInAction.RateLimitingExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var ratePolicy = builder.ConfigureRateLimiting();

var app = builder.Build();

//Use the rate limiter middleware
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers()
    .RequireRateLimiting(RateLimitingPolicyNames.Sliding);

static string GetTicks()
{
    return (DateTime.Now.Ticks & 0x11111).ToString("00000");
}

app.MapGet("/ratelimiting", () => Results.Ok($"Hello {GetTicks()}"))
    .RequireRateLimiting(ratePolicy);


app.MapGet("/jwt", (HttpContext context) => $"Hello {GetUserEndPointMethod(context)}")
    .RequireRateLimiting(RateLimitingPolicyNames.Jwt)
    .RequireAuthorization();

app.MapPost("/post", (HttpContext context) => $"Hello {GetUserEndPointMethod(context)}")
    .RequireRateLimiting(RateLimitingPolicyNames.Jwt)
    .RequireAuthorization();

app.Run();

static string GetUserEndPointMethod(HttpContext context) =>
    $"Hello {context.User.Identity?.Name ?? "Anonymous"} " +
    $"Endpoint:{context.Request.Path} Method: {context.Request.Method}";