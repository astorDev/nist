var builder = new BackgroundApplication.Builder(args);

builder.Services.AddBackgroundServiceDeclaration();
builder.Services.AddBackgroundServiceControllers();
builder.Services.AddTimerCollections();
builder.Services.AddSingleton<IHostedService, AtomicTimersHostedService>();
builder.Services.AddApplicationServices();

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);
builder.Logging.AddStateJsonConsole(j => j.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

var app = builder.Build();

app.Pipe.Use<IOLogger>();
app.Pipe.Use<HandlingTimer>();
app.Pipe.Use<ActionExceptionCatcher>();
app.Pipe.Use<ActionExecutor>();

await app.Run();

public static class AppServicesExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddElnikClient();
    }
}