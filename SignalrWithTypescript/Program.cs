using System.Net;
using SignalrWithTypescript.Hubs;
using SignalrWithTypescript.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR()
    // For .net client only
    .AddMessagePackProtocol()
    .AddStackExchangeRedis(options =>
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
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

//Signal R
app.MapHub<ChatHub>("/hub");

app.Run();
