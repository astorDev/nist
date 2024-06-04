using System.Net;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Nist.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = 
        HttpLoggingFields.RequestBody 
        | HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestBody
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestQuery
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.ResponseBody
        | HttpLoggingFields.Duration;

    o.RequestHeaders.Clear();
    o.ResponseHeaders.Clear();

    o.CombineLogs = true;
}).AddHttpLoggingInterceptor<SecretsInterceptor>();

var app = builder.Build();

app.UseHttpLogging();

app.UseErrorBody<Error>(ex => ex switch
{
    BadLookException => new(HttpStatusCode.BadRequest, "BadLook"),
    _ => new(HttpStatusCode.InternalServerError, "Unknown")
}, showException: false);

app.MapPost("parties/{id}/visitors", (string id, [FromQuery] bool goodLooking, Visitor visitor) =>
{
    if (!goodLooking)
    {
        throw new BadLookException();
    }

    return new Invitation(
        id,
        visitor.Name,
        new(visitor.Passcode.Reverse().ToArray())
    );
});

app.Run();

public record Visitor(string Name, string Passcode);
public record Invitation(string PartyId, string Receiver, string Code);
public class BadLookException : Exception { };

public class SecretsInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
        return default;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        logContext.Parameters.Remove(p => p.Value?.ToString() == "[Redacted]");
        logContext.Parameters.Remove("PathBase");
        logContext.Parameters.Remove("Protocol");
        logContext.Parameters.Remove("Scheme");
        logContext.Parameters.Remove("Content-Type");

        return default;
    }
}

public static class KeyValuePairList
{
    public static void Remove<TKey, TValue>(this IList<KeyValuePair<TKey, TValue>> list, Func<KeyValuePair<TKey, TValue>, bool> matcher)
    {
        var matching = list.Where(matcher).ToArray();
        foreach (var item in matching)
        {
            list.Remove(item);
        }
    }

    public static void Remove<TKey, TValue>(this IList<KeyValuePair<TKey, TValue>> list, TKey key)
    {
        list.Remove(p => p.Key!.Equals(key));
    }
}
