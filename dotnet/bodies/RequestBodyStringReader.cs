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