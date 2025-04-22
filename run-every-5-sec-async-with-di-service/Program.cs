var builder = WebApplication.CreateBuilder(args);

// Get environment
var env = builder.Environment;

builder.Services.AddScoped<IMyScopedService, MyScopedService>();

// Register InitService only for Production
if (env.IsProduction())
{
    builder.Services.AddHostedService<InitService>();
}

var app = builder.Build();

app.MapControllers(); // or app.MapGet, etc.

app.Run();
