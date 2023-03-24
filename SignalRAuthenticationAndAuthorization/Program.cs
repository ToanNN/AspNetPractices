using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRAuthenticationAndAuthorization.Data;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

builder.Services.AddSignalR().AddStackExchangeRedis(options =>
{
    options.Configuration.ChannelPrefix = "TnApp";

    options.ConnectionFactory = async writer =>
    {
        var config = new ConfigurationOptions()
        {
            AbortOnConnectFail = false,
        };
        config.EndPoints.Add(IPAddress.Loopback, 0);
        config.SetDefaultPorts();
        var connection = await ConnectionMultiplexer.ConnectAsync(config, writer);
        connection.ConnectionFailed += (_, e) =>
        {
            Console.WriteLine("Connection to Redis failed.");
        };

        if (!connection.IsConnected)
        {
            Console.WriteLine("Did not connect to Redis.");
        }

        return connection;
    };
});

builder.Services.AddAuthentication(options =>
    {
        // Identity made Cookie authentication the default.
        // However, we want JWT Bearer Auth to be the default.
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Configure the Authority to the expected value for
        // the authentication provider. This ensures the token
        // is appropriately validated.
        options.Authority = "Authority URL";

        // We have to hook the OnMessageReceived event in order to
        // allow the JWT authentication handler to read the access
        // token from the query string when a WebSocket or 
        // Server-Sent Events request comes in.

        // Sending the access token in the query string is required due to
        // a limitation in Browser APIs. We restrict it to only calls to the
        // SignalR hub in this code.
        // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
        // for more information about security considerations when using
        // the query string to transmit the access token.

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                //if the request is for our hubs
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/chat"))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();