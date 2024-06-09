using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Nist.Bodies;

public static class HttpIOMessagesRegistry
{
    public const string Method = "Method";
    public const string Uri = "Uri";
    public const string Endpoint = "Endpoint";
    public const string RequestBody = "RequestBody";
    public const string ResponseBody = "ResponseBody";
    public const string ResponseCode = "ResponseCode";
    public const string Elapsed = "Elapsed";
    public const string Exception = "Exception";

    public static readonly Dictionary<string, Func<HttpContext, object?>> DefaultExtractors = new()
    {
        { Method, context => context.Request.Method },
        { Uri, context => context.Request.GetEncodedPathAndQuery() },
        { Endpoint, ExtractEndpoint },
        { RequestBody, context => context.GetRequestBodyString() },
        { ResponseBody, context => context.GetResponseBodyString() },
        { ResponseCode, context => context.Response.StatusCode },
        { Elapsed, context => context.GetElapsed().TotalMilliseconds },
        { Exception, ExtractSafeException }
    };

    public static readonly HttpIOLogMessageSetting Default = new(
        template: LogMessageTemplate.Parse(@$"{{{Method}}} {{{Uri}}} > {{{ResponseCode}}} in {{{Elapsed}}}ms 
Endpoint: {{{Endpoint}}}
RequestBody: {{{RequestBody}}}
ResponseBody: {{{ResponseBody}}}
Exception: {{{Exception}}}"),
        valueExtractors: DefaultExtractors,
        beforeLoggingMiddleware: DefaultBeforeLoggingMiddleware,
        afterLoggingMiddleware: DefaultAfterLoggingMiddleware);

    public static readonly HttpIOLogMessageSetting Http =
        Default.CopyWith(
            logMessageTemplateString: @$"{{{Method}}} {{{Uri}}}

{{{RequestBody}}}

>>> 

{{{ResponseCode}}} {{{ResponseBody}}}

Elapsed: {{{Elapsed}}}
Endpoint: {{{Endpoint}}}
Exception: {{{Exception}}}
");

    public static readonly HttpIOLogMessageSetting DefaultWithJsonBodies = Default.CopyWith(
        valueExtractorsOverrides: d =>
        {
            d[RequestBody] = context => context.GetRequestBodyString().AsJsonObject();
            d[ResponseBody] = context => context.GetResponseBodyString().AsJsonObject();
        }
    );

    public static void DefaultBeforeLoggingMiddleware(IApplicationBuilder app)
    {
        app.UseRequestBodyStringReader();
    }

    public static void DefaultAfterLoggingMiddleware(IApplicationBuilder app)
    {
        app.UseElapsed();
        app.UseResponseBodyStringReader();
    }

    public static object? ExtractSafeException(HttpContext context)
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandler?.Error;
        return exception == null ? null : new { exception.Message, exception.StackTrace, InnerExceptionMessage = exception.InnerException?.Message };
    }

    public static object? ExtractEndpoint(HttpContext context)
    {
        var endpoint = context.GetEndpoint() ?? context.Features.Get<IExceptionHandlerPathFeature>()?.Endpoint;
        return endpoint;
    }

    public static object? AsJsonObject(this string raw)
    {
        if (String.IsNullOrWhiteSpace(raw)) return null;
        var jsonObject = JsonSerializer.Deserialize<JsonElement>(raw);
        return jsonObject;
    }
}