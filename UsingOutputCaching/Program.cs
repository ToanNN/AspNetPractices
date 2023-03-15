using System.Globalization;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Net.Http.Headers;
using UsingOutputCaching.FactoryBasedMiddlewareActivation;
using UsingOutputCaching.Middleware;
using UsingOutputCaching.Middleware.ScopeServices;
using UsingOutputCaching.UsingConfigurations;

var builder = WebApplication.CreateBuilder(args);
//Remove default logging providers
//Console
//Debug
//EventSource
//EventLog: Windows only
builder.Logging.ClearProviders();

//Add console loggin
builder.Logging.AddConsole();

builder.Services.AddResponseCaching(options =>
{
    // 64kb default 64Mb
    options.MaximumBodySize = 64*1024;

    //Default 100 Mb
    options.SizeLimit = 100*1024;
    //Default false
    options.UseCaseSensitivePaths = true;
});

// Add decompression services to to WebhostBuilder
builder.Services.AddRequestDecompression();

// This service will be shared with middleware InvokeAsync
builder.Services.AddScoped<IMessageWriter, ConsoleWriter>();
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

// Once instance per injection
builder.Services.AddTransient<FactoryActivatedMiddleware>();

builder.Services.Configure<PositionOptions>(builder.Configuration.GetSection(PositionOptions.Position));


//******************************************************************//

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

//Create an inline middleware
// we should prefer Use(HttpContext, RequestDelegate)
//app.Use(async (context, next) =>
//{
//    var cultureQuery = context.Request.Query["culture"];
//    if (!string.IsNullOrWhiteSpace(cultureQuery))
//    {
//        var culture = new CultureInfo(cultureQuery);
//        CultureInfo.CurrentCulture = culture;
//        CultureInfo.CurrentUICulture = culture;
//    }

//    await next(context);

//});


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

// Add the middleware to the app
app.UseRequestCulture();

// Add the request decompression middleware
// Request decompression uses "Content-Encoding" header to determine a decoder
// available values are "br", "deflate", and "gzip"
app.UseRequestDecompression();

//Register middleware with the request pipeline
app.UseConventionalMiddleware();
app.UseFactoryBasedMiddleware();

app.Run();
