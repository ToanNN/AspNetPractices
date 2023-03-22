using SignalrWithTypescript.Hubs;
using SignalrWithTypescript.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

//Signal R
app.MapHub<ChatHub>("/hub");

app.Run();
