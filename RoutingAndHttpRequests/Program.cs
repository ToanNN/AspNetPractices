using System.Text.Json;
using Microsoft.Net.Http.Headers;
using RoutingAndHttpRequests.RequestAndResponse;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("GitHub", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://api.github.com");
    httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/vnd.github.v3+json");
}).AddHttpMessageHandler<UserAgentHeaderHandler>();

builder.Services.AddTransient<UserAgentHeaderHandler>();
builder.Services.AddHostedService<PeriodicBranchLoggerService>();

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

//app.MapGet("/branches",
//    async (IHttpClientFactory clientFactory, HttpContext context, Logger<Program> logger) =>
//    {
//        var httpClient = clientFactory.CreateClient("GitHub");
//        var responseMessage = await httpClient.GetAsync("repos/dotnet/AspNetCore.Docs/branches");
//        if (!responseMessage.IsSuccessStatusCode)
//            return Results.BadRequest();
//        await using var contentStream = await responseMessage.Content.ReadAsStreamAsync();

//        var response = await JsonSerializer.DeserializeAsync<IEnumerable<GitHubBranch>>(contentStream);
//        app.Logger.LogInformation("/branches request: " +
//                                  $"{JsonSerializer.Serialize(response)}");

//        return Results.Ok(response);
//    });

app.Run();

public class GitHubBranch
{
}