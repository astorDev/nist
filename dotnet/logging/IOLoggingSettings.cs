using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http.Extensions;

namespace Nist.Logs;

public class IOLoggingSettings
{
    public class Fields
    {
        public const string Uri = "uri";
        public const string Endpoint = "endpoint";
        public const string RequestBody = "requestBody";
        public const string ResponseBody = "responseBody";
        public const string ResponseCode = "responseCode";
        public const string Elapsed = "elapsed";
        public const string Exception = "exception";
        public const string Method = "method";
    }

    public List<string> LoggedFields { get; } = new()
    {
        Fields.Uri,
        Fields.Endpoint,
        Fields.RequestBody,
        Fields.ResponseBody,
        Fields.ResponseCode,
        Fields.Elapsed,
        Fields.Exception,
        Fields.Method
    };

    public List<string> Ignored { get; } = new();

    public readonly Dictionary<string, Func<object, object>> Formatters = new()
    {
        { Fields.Uri, AsIs },
        { Fields.Endpoint, ToLowerString },
        { Fields.RequestBody,  AsIs },
        { Fields.ResponseBody,  AsIs },
        { Fields.ResponseCode,  AsIs },
        { Fields.Elapsed,  GetMilliseconds },
        { Fields.Exception,  ToSafeException },
        { Fields.Method,  AsIs }
    };

    public static object GetMilliseconds(object source) => ((TimeSpan)source).TotalMilliseconds;

    public static object AsIs(object source) => source;

    public static object ToLowerString(object source) => source.ToString()!.ToLower();

    public static object ToSafeException(Exception source) => new { source.Message, source.StackTrace, InnerExceptionMessage = source.InnerException?.Message };

    public static object ToSafeException(object source) => ToSafeException((Exception)source);

    public bool Ignores(HttpContext context) => 
        this.Ignored.Any(p => Regex.IsMatch(context.Request.Path.ToString(), p));

    public Dictionary<string, object> GetLoggedParams(HttpIOInformation info)
    {
        var result = new Dictionary<string, object>();
        
        void MaybeAdd(string key, object? value)
        {
            if (!this.LoggedFields.Contains(key) || value == null) return;

            var formatter = this.Formatters[key];
            result.Add(key, formatter(value));
        }
        
        var uri = info.HttpContext.Request.GetEncodedPathAndQuery();
        var endpoint = info.HttpContext.GetEndpoint();
        
        MaybeAdd(Fields.Uri, uri);
        MaybeAdd(Fields.Endpoint, endpoint);
        MaybeAdd(Fields.RequestBody, info.RequestBody);
        MaybeAdd(Fields.ResponseBody, info.ResponseBody);
        MaybeAdd(Fields.ResponseCode, info.HttpContext.Response.StatusCode);
        MaybeAdd(Fields.Elapsed, info.Elapsed);
        MaybeAdd(Fields.Exception, info.Exception);
        MaybeAdd(Fields.Method, info.HttpContext.Request.Method);

        return result;
    }
}