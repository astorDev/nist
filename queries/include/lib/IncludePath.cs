using QueryEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>>;

namespace Nist;

public record IncludeQueryParameter(
    IncludePath[] Paths
)
{
    public static IncludeQueryParameter Parse(string value)
    {
        var rawValues = value.Split(",");
        var paths = rawValues.Select(IncludePath.Parse);

        return new IncludeQueryParameter([.. paths]);
    }

    public static IncludeQueryParameter? Search(QueryEnumerable query)
    {
        var raw = query.SearchString("include");
        return raw == null ? null : Parse(raw);
    }

    public bool Contains(string key) => TryGetSubpath(key, out _);
    public bool TryGetSubpath(string key, out IncludePath? subpath)
    {
        var path = Paths.FirstOrDefault(p => p.Root == key);
        subpath = path?.Subpath;
        return path != null;
    }

    public IEnumerable<IncludePath> GetSubpathes(string key) => Paths.GetSubpathes(key);
}

public static class IncludePathEnumerableExtensions
{
    public static IEnumerable<IncludePath> GetSubpathes(this IEnumerable<IncludePath> pathes, string key) => 
        pathes
            .Where(p => p.Root == key && p.Subpath != null)
            .Select(p => p.Subpath!);

    public static bool Have(this IEnumerable<IncludePath> pathes, string key) => 
        pathes.Any(p => p.Root == key);
}

public record IncludePath(
    string Root,
    IncludePath? Subpath
)
{
    public static IncludePath Parse(string value) => 
        Parse(value.Split("."));

    public static IncludePath Parse(string[] parts) => new(
        Root: parts[0],
        Subpath: parts.Length > 1 ? IncludePath.Parse([.. parts.Skip(1)]) : null
    );
}
