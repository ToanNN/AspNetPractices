var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("hostsettings.json", true, true)
    .AddCommandLine(args)
    .Build();

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://*.5000")
    .UseConfiguration(config);
var app = builder.Build();


app.MapGet("/", () => "Hello World!");


app.Run();