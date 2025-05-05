namespace Nist;

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

    override public string ToString()
    {
        return Subpath != null ? $"{Root}.{Subpath}" : Root;
    }
}

public record IncludeQueryParameter(
    IncludePath[] Paths
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
        var paths = rawValues.Select(IncludePath.Parse);
        return new IncludeQueryParameter([.. paths]);
    }

    public bool Contains(string key) => TryGetSubpath(key, out _);
    public bool TryGetSubpath(string key, out IncludePath? subpath)
    {
        var path = Paths.FirstOrDefault(p => p.Root == key);
        subpath = path?.Subpath;
        return path != null;
    }

    public IEnumerable<IncludePath> GetSubpathes(string key) => Paths.GetSubpathes(key);

    public override string ToString()
    {
        return string.Join(",", Paths.Select(p => p.ToString()));
    }
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