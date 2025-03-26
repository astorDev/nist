namespace Nist;

public class ResponseBodyStringReader(RequestDelegate next) 
{
    public const string Key = "responseBody";

    public async Task Invoke(HttpContext context)
    {
        var originalStream = context.Response.Body;

        await using var tempStream = new MemoryStream();
        context.Response.Body = tempStream;

        await next(context);

        tempStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(tempStream).ReadToEndAsync();
        
        tempStream.Seek(0, SeekOrigin.Begin);
        await tempStream.CopyToAsync(originalStream);

        context.Items.Add(Key, responseBody);
    }
}

public static class ResponseBodyStringReaderRegistration
{
    public static bool registered = false;

    public static void UseResponseBodyStringReader(this IApplicationBuilder app, DuplicateRegistrationBehavior duplicatingRegistrationBehavior = DuplicateRegistrationBehavior.Ignore)
    {
        DuplicateRegistrationDecorator.HandleRegistration(
            app, 
            nameof(ResponseBodyStringReader), 
            (x) => x.UseMiddleware<ResponseBodyStringReader>(), 
            duplicatingRegistrationBehavior
        );
    }
}

public static class ResponseBodyStringReaderExtensions
{
    public static string GetResponseBodyString(this HttpContext context)
    {
        return context.Items[ResponseBodyStringReader.Key] as string ?? throw new InvalidOperationException("Response body is not available. Make sure you've registered the required middleware with `UseResponseBodyStringReader()`");
    }
}