using MakingHttpRequests.Pages;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages();

// Create a named client for Github
builder.Services.AddHttpClient("GitHub", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/vnd.github.v3+json");
    client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "higher-longer-fed");
});

// Register a typed client
//This registration uses a factory method to:

//Create an instance of HttpClient.
//    Create an instance of GitHubService, passing in the instance of HttpClient to its constructor.
// The instance is transient and has an instance of HttpClient injected in
builder.Services.AddHttpClient<GitHubService>()
    // Keep the HttpMessageHandler instance for 5 minutes instead of 2 minutes (default)
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    //Configure the HttpMessageHandler
    .ConfigurePrimaryHttpMessageHandler(()=> new HttpClientHandler()
    {
        AllowAutoRedirect = true,
        UseDefaultCredentials = true,
        UseCookies = true
    });



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
