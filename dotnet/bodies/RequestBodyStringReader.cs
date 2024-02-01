namespace Nist.Bodies;

public class RequestBodyStringReader(RequestDelegate next)
{
    public const string Key = "requestBody";

    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();
        await using var ms = new MemoryStream();
        await context.Request.Body.CopyToAsync(ms);
        ms.Position = 0;
        var requestBody = await new StreamReader(ms).ReadToEndAsync();
        
        context.Request.Body.Position = 0;
        context.Items.Add(Key, requestBody);

        await next(context);
    }
}

public static class RequestBodyStringReaderRegistration
{
    public static void UseRequestBodyStringReader(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestBodyStringReader>();
    }
}

public static class RequestBodyStringReaderExtensions
{
    public static string GetRequestBodyString(this HttpContext context)
    {
        return context.Items[RequestBodyStringReader.Key] as string ?? throw new InvalidOperationException("Request body is not available. Make sure you've registered the required middleware with `UseRequestBodyStringReader()`");
    }
}