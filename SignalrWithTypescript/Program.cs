using MessagePack;
using SignalrWithTypescript.Hubs;
using SignalrWithTypescript.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
    // For .net client only
    //.AddMessagePackProtocol(options =>
    //{
    //    options.SerializerOptions = MessagePackSerializerOptions.Standard
    //        .WithSecurity(MessagePackSecurity.UntrustedData);
    //});

builder.Services.AddScoped<IDatabaseService, DatabaseService>();
var app = builder.Build();

// when you browse the website address, it will direct to the default file
app.UseDefaultFiles();
app.UseStaticFiles();

//Signal R
app.MapHub<ChatHub>("/hub");

app.Run();