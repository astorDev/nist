using System.Text.RegularExpressions;

namespace Nist;

public record HttpIOLogMessageSetting
{
    readonly LogMessageTemplate template;
    readonly IReadOnlyDictionary<string, Func<HttpContext, object?>> valueExtractors;
    public readonly Action<IApplicationBuilder>? BeforeLoggingMiddleware;
    public readonly Action<IApplicationBuilder>? AfterLoggingMiddleware;

    public HttpIOLogMessageSetting(
        LogMessageTemplate template, 
        IReadOnlyDictionary<string, Func<HttpContext, object?>> valueExtractors,
        Action<IApplicationBuilder>? beforeLoggingMiddleware = null,
        Action<IApplicationBuilder>? afterLoggingMiddleware = null
    )
    {
        foreach (var key in template.OrderedKeys)
        {
            if (!valueExtractors.ContainsKey(key))
            {
                throw new ArgumentException($"Value extractor for key '{key}' not found");
            }
        }

        this.valueExtractors = valueExtractors;
        this.template = template;
        this.BeforeLoggingMiddleware = beforeLoggingMiddleware;
        this.AfterLoggingMiddleware = afterLoggingMiddleware;
    }

    public void Log(HttpContext context, ILogger logger)
    {
        var logParameters = template.OrderedKeys.Select(k => valueExtractors[k](context));
        
        logger.LogInformation(template.Message, logParameters.ToArray());
    }

    public HttpIOLogMessageSetting CopyWith(string? logMessageTemplateString = null, Action<Dictionary<string, Func<HttpContext, object?>>>? valueExtractorsOverrides = null)
    {
        var newValueExtractor = new Dictionary<string, Func<HttpContext, object?>>(valueExtractors);
        valueExtractorsOverrides?.Invoke(newValueExtractor);

        return new(
            logMessageTemplateString == null ? template : LogMessageTemplate.Parse(logMessageTemplateString),
            newValueExtractor,
            BeforeLoggingMiddleware,
            AfterLoggingMiddleware
        );
    }
}

public partial record LogMessageTemplate
{
    public readonly string Message;
    public readonly string[] OrderedKeys;

    private LogMessageTemplate(string message, string[] orderedKeys)
    {
        Message = message;
        OrderedKeys = orderedKeys;
    }

    static readonly Regex TemplateKeyRegex = new(@"\{(\w+)\}");

    public static LogMessageTemplate Parse(string template)
    {
        var keys = new List<string>();
        var matches = TemplateKeyRegex.Matches(template);
        foreach (Match match in matches)
        {
            var value = match.Groups[1].Value;
            keys.Add(value);
        }

        return new(template, [.. keys]);
    }
}