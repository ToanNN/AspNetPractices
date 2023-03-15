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

//Blocking the main thread until the app is shut down

Console.WriteLine("Use Ctrl-C to shutdown the host...");
await app.RunAsync();

//WebHost.Start will not block the main thread
// you have to call host.WaitForShutdown();