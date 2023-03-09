using AspnetCoreFeatures.OptionPattern;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);


var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
}).CreateLogger("Program");


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddEnvironmentVariables(prefix: "NgocKem_");

//Add to the container as singleton so can inject to any services
// do not support reading configuration data after the app has started
builder.Services.Configure<Position>(builder.Configuration.GetSection(Position.ConfigSectionName));

//configure HTTP Logging middleware
builder.Services.AddHttpLogging(loggingOptions =>
{
    loggingOptions.LoggingFields = HttpLoggingFields.All;
    //Request header to be logged
    loggingOptions.RequestHeaders.Add("sec-ch-ua");
    //response headers to be logged
    loggingOptions.ResponseHeaders.Add("App-Response-Header");
    // Logging data for javascript
    loggingOptions.MediaTypeOptions.AddText("application/javascript");
    loggingOptions.RequestBodyLogLimit = 4096;
    loggingOptions.ResponseBodyLogLimit = 4096;
});


var app = builder.Build();

//app.UseHttpLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Starting the development environment");
    app.UseSwagger();
    app.UseSwaggerUI();
}

//PrintConfigurationSettings(builder);



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Check the request properties
app.MapGet("/requestctx", (HttpRequest request,HttpResponse response) =>
{
    var userAgent = request.Headers.UserAgent;
    var customHeader = request.Headers["x-custom-header"];
    var method = request.Method;
    var routedValues = request.RouteValues;

    Console.WriteLine($"User Agent = {userAgent}, Custom Header {customHeader}, Method {method}");
    foreach (var routedValue in routedValues)
    {
        Console.WriteLine($"Key = {routedValue.Key}, Value = {routedValue.Value}");
    }

    response.Headers.CacheControl = "no-cache";
    response.Headers["x-custom-header"] = "Custom value";
});

//Enable buffering allows middleware to read a request multiple time

app.Use(async (ctx, next) =>
{
    ctx.Request.EnableBuffering();

    await ReadRequestBody(ctx.Request.Body);

    ctx.Request.Body.Position = 0;
    await next.Invoke();

    async Task ReadRequestBody(Stream requestBody)
    {

    }
});

app.MapWhen(context => context.Request.Query.ContainsKey("branch"), HandleBranch);

app.Run();


void PrintConfigurationSettings(WebApplicationBuilder webApplicationBuilder)
{
//Display all environment variables
    foreach (var c in webApplicationBuilder.Configuration.AsEnumerable())
    {
        Console.WriteLine(c.Key + " = " + c.Value);
    }

// Get configuration options from the config file
    var positionOptions = webApplicationBuilder.Configuration.GetSection(Position.ConfigSectionName).Get<Position>();

    Console.WriteLine($"Title: {positionOptions.Title} \n" +
                      $"Name: {positionOptions.Name}");
}


static void HandleBranch(IApplicationBuilder app)
{
    app.Run(async context =>
    {
        var branchVersion = context.Request.Query["branch"];
        await context.Response.WriteAsync($"Branch used = {branchVersion}");
    });
}