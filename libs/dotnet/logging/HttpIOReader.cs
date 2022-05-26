using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace Nist.Logs;

public static class HttpIOReader
{
    public static async Task<HttpIOInformation> ExecuteAndGetInformation(HttpContext context, RequestDelegate requestDelegate)
    {
        var requestBody = await GetRequestBody(context);
        var (elapsed, responseBody) = await GetResponseParams(context, requestDelegate);
        
        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
        return new(
            context,
            requestBody,
            responseBody,
            elapsed,
            exceptionHandler?.RouteValues ?? context.Request.RouteValues,
            exceptionHandler?.Error
        );
    }
    
    public static async Task<(TimeSpan Elapsed, string Body)> GetResponseParams(HttpContext context, RequestDelegate next)
    {
        var stopwatch = new Stopwatch();
        string responseBody;
        var originalResponseStream = context.Response.Body;
        
        await using (var responseStream = new MemoryStream())
        {
            context.Response.Body = responseStream;
            stopwatch.Start();
            await next(context);
            stopwatch.Stop();

            context.Response.Body.Seek(0, SeekOrigin.Begin);

            responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();

            context.Response.Body.Seek(0, SeekOrigin.Begin);

            await responseStream.CopyToAsync(originalResponseStream);
        }

        return (stopwatch.Elapsed, responseBody);
    }

    public static async Task<string> GetRequestBody(HttpContext context)
    {
        string requestBody;
        context.Request.EnableBuffering();
        await using (var ms = new MemoryStream())
        {
            await context.Request.Body.CopyToAsync(ms);
            ms.Position = 0;
            requestBody = await new StreamReader(ms).ReadToEndAsync();
        }
        context.Request.Body.Position = 0;
        return requestBody;
    }
}
