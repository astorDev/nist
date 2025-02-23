namespace Nist;

public class HttpIOLoggingMiddleware(
    RequestDelegate next,
    HttpIOLoggingSettings settings,
    ILogger<HttpIOLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        await next(context);
        
        if (!settings.Ignores(context))
        {
            settings.Message.Log(context, logger);    
        }
    }
}

public static class RequestsLoggingRegistration
{
    public static void UseHttpIOLogging(this IApplicationBuilder app, Action<HttpIOLoggingSettings>? configuration = null)
    {
        var settings = new HttpIOLoggingSettings();
        configuration?.Invoke(settings);

        settings.Message.BeforeLoggingMiddleware?.Invoke(app);
        app.UseMiddleware<HttpIOLoggingMiddleware>(settings);
        settings.Message.AfterLoggingMiddleware?.Invoke(app);
    }
}