using SignalrWithTypescript.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

//Signal R
app.MapHub<ChatHub>("/hub");

app.Run();
