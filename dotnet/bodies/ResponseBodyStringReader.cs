namespace Nist.Bodies;

public class ResponseBodyStringReader(RequestDelegate next) 
{
    public const string Key = "responseBody";

    public async Task Invoke(HttpContext context)
    {
        var originalResponseStream = context.Response.Body;
        await using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;
        await next(context);
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        await responseStream.CopyToAsync(originalResponseStream);
        context.Items.Add(Key, responseBody);
    }
}

public static class ResponseBodyStringReaderRegistration
{
    public static void UseResponseBodyStringReader(this IApplicationBuilder app)
    {
        app.UseMiddleware<ResponseBodyStringReader>();
    }
}