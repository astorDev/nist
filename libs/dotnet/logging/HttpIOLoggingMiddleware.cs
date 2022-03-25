using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Nist.Logs;

public class HttpIOLoggingMiddleware
{
    public record Settings()
    {
        public List<string> IgnoredPathPatterns { get; } = new List<string>();
        public bool Ignores(HttpContext context) => this.IgnoredPathPatterns.Any(p => Regex.IsMatch(context.Request.Path.ToString(), p));
    }

    public RequestDelegate Next { get; }
    public Settings Configuration { get; }
    public ILogger<HttpIOLoggingMiddleware> Logger { get; }
    public HttpIOLoggingMiddleware(RequestDelegate next, Settings configuration, ILogger<HttpIOLoggingMiddleware> logger)
    {
        this.Next = next;
        this.Configuration = configuration;
        this.Logger = logger;
    }
    
    public async Task Invoke(HttpContext context)
    {
        if (this.Configuration.Ignores(context))
        {
            await this.Next(context);
            return;
        }

        var requestBody = await HttpIOReader.GetRequestBody(context);
        var (elapsed, body) = await HttpIOReader.GetResponseParams(context, this.Next);
        
        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
            this.Logger.LogInformation(@"{method}
{uri}
{request}
{responseCode}
{responseBody}
{elapsed},
{exception}",
            context.Request.Method,
            context.Request.GetDisplayUrl(),
            requestBody,
            context.Response.StatusCode,
            body,
            elapsed, 
            exceptionHandler?.Error.ToString());
    }
}

public static class RequestsLoggingRegistration
{
    public static void UseHttpIOLogging(this IApplicationBuilder app, Action<HttpIOLoggingMiddleware.Settings>? configuration = null)
    {
        var settings = new HttpIOLoggingMiddleware.Settings();
        configuration?.Invoke(settings);
        app.UseMiddleware<HttpIOLoggingMiddleware>(settings);
    }
}