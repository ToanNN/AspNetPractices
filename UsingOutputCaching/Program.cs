using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddResponseCaching(options =>
{
    // 64kb default 64Mb
    options.MaximumBodySize = 64*1024;

    //Default 100 Mb
    options.SizeLimit = 100*1024;
    //Default false
    options.UseCaseSensitivePaths = true;
});

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Must be called before use Response Caching
app.UseCors();

app.UseResponseCaching();

app.Use(async (ctx, next) =>
{
    ctx.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
    {
        Public = true,
        MaxAge = TimeSpan.FromSeconds(10)
    };

    ctx.Response.Headers[HeaderNames.Vary] = new[] { "Accept-Encoding" };
    await next();
});

app.Run();
