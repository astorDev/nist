using Nist.Bodies;

namespace Nist.Logs;

public class HttpIOLoggingMiddleware(
    RequestDelegate next,
    IOLoggingSettings settings,
    ILogger<HttpIOLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        if (settings.Ignores(context))
        {
            await next(context);
            return;
        }

        var information = await HttpIOReader.ExecuteAndGetInformation(context, next);
        var loggedParams = settings.GetLoggedParams(information);

        logger.LogInformation(settings.Template.Message, loggedParams.ToArray());
    }
}

public static class RequestsLoggingRegistration
{
    public static void UseHttpIOLogging(this IApplicationBuilder app, Action<IOLoggingSettings>? configuration = null)
    {
        var settings = new IOLoggingSettings();
        configuration?.Invoke(settings);

        if (settings.Template.OrderedKeys.Contains(IOLoggingSettings.Fields.RequestBody))
        {
            app.UseRequestBodyStringReader();
        }

        app.UseMiddleware<HttpIOLoggingMiddleware>(settings);

        if (settings.Template.OrderedKeys.Contains(IOLoggingSettings.Fields.ResponseBody))
        {
            app.UseResponseBodyStringReader();
        }
    }
}