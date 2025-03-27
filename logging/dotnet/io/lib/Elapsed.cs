using System.Diagnostics;

namespace Nist;

public class ElapsedMiddleware(RequestDelegate next)
{
    public const string ElapsedKey = "elapsed";

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        await next(context);
        stopwatch.Stop();
        context.Items[ElapsedKey] = stopwatch.Elapsed;
    }
}

public static class ElapsedMiddlewareExtensions
{
    public static IApplicationBuilder UseElapsed(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ElapsedMiddleware>();
    }

    public static TimeSpan GetElapsed(this HttpContext context)
    {
        return (TimeSpan)context.Items[ElapsedMiddleware.ElapsedKey]!;
    }
}