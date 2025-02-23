using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace Nist;

public static class HttpIOReader
{
    public static async Task<HttpIOInformation> ExecuteAndGetInformation(HttpContext context, RequestDelegate requestDelegate)
    {
        context.Items.TryGetValue("requestBody", out var requestBody);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await requestDelegate(context);
        stopwatch.Stop();
        context.Items.TryGetValue("responseBody", out var responseBody);
        
        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
        return new(
            context,
            requestBody?.ToString() ?? String.Empty,
            responseBody?.ToString() ?? String.Empty,
            stopwatch.Elapsed,
            exceptionHandler?.RouteValues ?? context.Request.RouteValues,
            exceptionHandler?.Error
        );
    }
}
