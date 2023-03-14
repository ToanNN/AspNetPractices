var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    //Customise web host to specify the application name and the root folder
    Args = args,
    ApplicationName = typeof(Program).Assembly.FullName,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "public"
});
var app = builder.Build();

app.MapGet("/", () => "Hello World!");


app.Run();
