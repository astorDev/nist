using QueryEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>;

namespace Nist;

public static class QueryCollectionExtensions
{
    public static int? SearchInt(this QueryEnumerable query, string name)
    {
        var value = query.SearchString(name);
        return value == null ? null : int.Parse(value);
    }

    public static string? SearchString(this QueryEnumerable query, string name)
    {
        var value = query.FirstOrDefault(q => q.Key == name);
        return value.Key == null ? null : value.Value.First();
    }

    public static DateTime? SearchDateTime(this QueryEnumerable query, string name)
    {
        var value = query.SearchString(name);
        return value == null ? null : DateTime.Parse(value).ToUniversalTime();
    }
}