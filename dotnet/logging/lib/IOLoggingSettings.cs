using System.Text.RegularExpressions;

namespace Nist.Logs;

public class HttpIOLoggingSettings
{
    public HttpIOLogMessageSetting Message { get; set; } = HttpIOMessagesRegistry.Default;
    public List<string> Ignored { get; } = new();

    public bool Ignores(HttpContext context) => 
        this.Ignored.Any(p => Regex.IsMatch(context.Request.Path.ToString(), p));
}