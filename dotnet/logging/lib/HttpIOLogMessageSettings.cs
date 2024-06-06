using System.Text.RegularExpressions;

public record HttpIOLogMessageSetting
{
    readonly LogMessageTemplate template;
    readonly Dictionary<string, Func<HttpContext, object?>> valueExtractors;
    public readonly Action<IApplicationBuilder>? BeforeLoggingMiddleware;
    public readonly Action<IApplicationBuilder>? AfterLoggingMiddleware;

    public HttpIOLogMessageSetting(
        LogMessageTemplate template, 
        Dictionary<string, Func<HttpContext, object?>> valueExtractors,
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

    public HttpIOLogMessageSetting WithTemplateString(string logMessageTemplateString)
    {
        return new(
            LogMessageTemplate.Parse(logMessageTemplateString), 
            valueExtractors,
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