using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http.Extensions;

namespace Nist.Logs;

public partial class IOLoggingSettings
{
    public partial record MessageTemplate(string Message, string[] OrderedKeys)
    {
        static readonly Regex TemplateKeyRegex = new(@"\{(\w+)\}");
        
        public static MessageTemplate Parse(string template)
        {
            var keys = new List<string>();
            var matches = TemplateKeyRegex.Matches(template);
            foreach (Match match in matches)
            {
                var value = match.Groups[1].Value;
                if (!Fields.All.Contains(value))
                {
                    throw new ArgumentException($"Invalid template key: '{value}'. Valid keys are: {Environment.NewLine} {string.Join(Environment.NewLine, Fields.All)}");
                }

                keys.Add(value);
            }

            return new MessageTemplate(template, [.. keys]);
        }
    }

    public class Fields
    {
        public const string Uri = "Uri";
        public const string Endpoint = "Endpoint";
        public const string RequestBody = "RequestBody";
        public const string ResponseBody = "ResponseBody";
        public const string ResponseCode = "ResponseCode";
        public const string Elapsed = "Elapsed";
        public const string Exception = "Exception";
        public const string Method = "Method";

        public readonly static ISet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Uri,
            Endpoint,
            RequestBody,
            ResponseBody,
            ResponseCode,
            Elapsed,
            Exception,
            Method
        };

        public static Dictionary<string, Func<HttpIOInformation, object?>> ValueExtractors = new(StringComparer.OrdinalIgnoreCase)
        {
            { Uri, info => info.HttpContext.Request.GetEncodedPathAndQuery() },
            { Endpoint, info => info.HttpContext.GetEndpoint() },
            { RequestBody, info => info.RequestBody },
            { ResponseBody, info => info.ResponseBody },
            { ResponseCode, info => info.HttpContext.Response.StatusCode },
            { Elapsed, info => info.Elapsed },
            { Exception, info => info.Exception },
            { Method, info => info.HttpContext.Request.Method }
        };
    }

    public MessageTemplate Template { get; set; } = MessageTemplate.Parse(@"{Method} {Uri} > {ResponseCode} in {Elapsed}ms 
Endpoint: {Endpoint}
RequestBody: {RequestBody}
ResponseBody: {ResponseBody}
Exception: {Exception}");

    public List<string> Ignored { get; } = new();

    public readonly Dictionary<string, Func<object?, object?>> Formatters = new()
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

    public static object? GetMilliseconds(object? source) => ((TimeSpan)source!).TotalMilliseconds;

    public static object? AsIs(object? source) => source;

    public static object? ToLowerString(object? source) => source?.ToString()!.ToLower();

    public static object? ToSafeException(Exception? source) => source == null ? null : new { source.Message, source.StackTrace, InnerExceptionMessage = source.InnerException?.Message };

    public static object? ToSafeException(object? source) => ToSafeException(source == null ? (Exception?)null : (Exception?)source);

    public bool Ignores(HttpContext context) => 
        this.Ignored.Any(p => Regex.IsMatch(context.Request.Path.ToString(), p));

    public IEnumerable<object?> GetLoggedParams(HttpIOInformation info)
    {
        foreach (var key in this.Template.OrderedKeys)
        {
            if (!Fields.ValueExtractors.TryGetValue(key, out Func<HttpIOInformation, object?>? extractor))
            {
                throw new InvalidOperationException($"Extractor for key '{key}' not found");
            }

            var value = extractor(info);

            if (!Formatters.TryGetValue(key, out Func<object?, object?>? formatter))
            {
                throw new InvalidOperationException($"Formatter for key '{key}' not found");
            }

            yield return formatter(value);;
        }
    }
}