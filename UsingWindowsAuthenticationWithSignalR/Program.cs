using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.SignalR;
using UsingWindowsAuthenticationWithSignalR.CustomAuthorizationHandlers;
using UsingWindowsAuthenticationWithSignalR.CustomProviders;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
// Add services to the container.

services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});
services.AddRazorPages();


services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
// extract Name claim instead of Name Identifier claim
services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

services.AddAuthorization(options =>
    options.AddPolicy("DomainRestrictedPolicy", policy => policy.Requirements.Add(new DomainRestrictedRequirement())));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Client code (javascript code) is hosted at https://example.com
// SignalR backend code is hosted at https://signalr.example.com
// Allow clients to call signalR hubs 

app.UseCors(builder =>
{
    builder.WithOrigins("https://example.com")
        .AllowAnyHeader()
        .WithMethods("GET", "POST")
        .AllowCredentials();
});
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
