namespace Nist.Logs;

public class HttpIOLoggingMiddleware
{
    public RequestDelegate Next { get; }
    public IOLoggingSettings Settings { get; }
    public ILogger<HttpIOLoggingMiddleware> Logger { get; }
    
    public HttpIOLoggingMiddleware(
        RequestDelegate next, 
        IOLoggingSettings settings, 
        ILogger<HttpIOLoggingMiddleware> logger)
    {
        this.Next = next;
        this.Settings = settings;
        this.Logger = logger;
    }
    
    public async Task Invoke(HttpContext context)
    {
        if (this.Settings.Ignores(context))
        {
            await this.Next(context);
            return;
        }

        var information = await HttpIOReader.ExecuteAndGetInformation(context, this.Next);
        var loggedParams = this.Settings.GetLoggedParams(information);

        var message = String.Join(" ", loggedParams.Keys.Select(k => $"{{{k}}}"));
        
        this.Logger.LogInformation(message, loggedParams.Values.ToArray());
    }
}

public static class RequestsLoggingRegistration
{
    public static void UseHttpIOLogging(this IApplicationBuilder app, Action<IOLoggingSettings>? configuration = null)
    {
        var settings = new IOLoggingSettings();
        configuration?.Invoke(settings);
        app.UseMiddleware<HttpIOLoggingMiddleware>(settings);
    }
}