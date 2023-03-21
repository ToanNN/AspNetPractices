var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer");

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin_greeting_policy", policy => policy.RequireRole("admin")
        .RequireClaim("greetings_api"));


var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapGet("/hello", () => "Hello world")
    .RequireAuthorization("admin_greeting_policy");

app.Run();
