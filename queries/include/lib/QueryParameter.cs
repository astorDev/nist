namespace Nist;

public record IncludeQueryParameter(
    ObjectPath[] Paths
)
{
    public static bool TryParse(string source, out IncludeQueryParameter includeQueryParameter)
    {
        includeQueryParameter = Parse(source);
        return true;
    }

    public static IncludeQueryParameter Parse(string source)
    {
        var rawValues = source.Split(",");
        var paths = rawValues.Select(x => ObjectPath.Parse(x));
        return new IncludeQueryParameter([.. paths]);
    }

    public bool Contains(string key) => Paths.Have(key);

    public IEnumerable<ObjectPath> GetChildren(string key) => Paths.GetChildren(key);

    public override string ToString()
    {
        return string.Join(",", Paths.Select(p => p.ToString()));
    }
}

public static class IncludePathEnumerableExtensions
{
    public static IEnumerable<ObjectPath> GetChildren(this IEnumerable<ObjectPath> pathes, string key) =>
        pathes
            .Where(p => p.Root == key && p.Child != null)
            .Select(p => p.Child!);

    public static bool Have(this IEnumerable<ObjectPath> pathes, string key) =>
        pathes.Any(p => p.Root == key);
}