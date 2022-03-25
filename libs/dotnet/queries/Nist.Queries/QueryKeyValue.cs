using System.Net;

namespace Nist.Queries;

public record QueryKeyValue(string Key, string Value)
{
    public override string ToString()
    {
        return $"{this.Key}={this.Value}";
    }

    public static QueryKeyValue From(string key, object value) => value switch
    {
        DateTime time => new(key, time.ToString("O")),
        _ => new(key, WebUtility.UrlEncode(value.ToString())!)
    };
}