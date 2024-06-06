using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Nist.Bodies;

public class HttpIOMessagesRegistry
{
    public const string Method = "Method";
    public const string Uri = "Uri";
    public const string Endpoint = "Endpoint";
    public const string RequestBody = "RequestBody";
    public const string ResponseBody = "ResponseBody";
    public const string ResponseCode = "ResponseCode";
    public const string Elapsed = "Elapsed";
    public const string Exception = "Exception";

    public static readonly LogMessageTemplate DefaultTemplate = LogMessageTemplate.Parse(@$"{{{Method}}} {{{Uri}}} > {{{ResponseCode}}} in {{{Elapsed}}}ms 
Endpoint: {{{Endpoint}}}
RequestBodyString: {{{RequestBody}}}
ResponseBodyString: {{{ResponseBody}}}
Exception: {{{Exception}}}");

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

    public static Endpoint? ExtractEndpoint(HttpContext context)
    {
        return context.GetEndpoint() ?? context.Features.Get<IExceptionHandlerPathFeature>()?.Endpoint;
    }

    public static readonly HttpIOLogMessageSetting Default = new(
        template: DefaultTemplate,
        valueExtractors: DefaultExtractors,
        beforeLoggingMiddleware: DefaultBeforeLoggingMiddleware,
        afterLoggingMiddleware: DefaultAfterLoggingMiddleware);
    
    public static readonly HttpIOLogMessageSetting Http = 
        Default.WithTemplateString(
            @$"{{{Method}}} {{{Uri}}}

{{{RequestBody}}}

>>> 

{{{ResponseCode}}} {{{ResponseBody}}}

Elapsed: {{{Elapsed}}}
Endpoint: {{{Endpoint}}}
Exception: {{{Exception}}}
"
        );
}