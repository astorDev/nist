namespace Nist;

public record IncludeExpression(
    string Operator,
    string Value
)
{
    public static bool TryParse(string includePath, out IncludeExpression expression)
    {
        expression = null!;
        var parts = includePath.Split("_");
        if (parts.Length != 2) return false;

        expression = new IncludeExpression(
            parts[0],
            parts[1]
        );

        return true;
    }

    public static IncludeExpression? Search(string includePath)
    {
        if (TryParse(includePath, out var expression))
            return expression;

        return null;
    }
}

public static class IncludePathExpressionExtensions
{
    public static Dictionary<IncludeExpression, IEnumerable<IncludePath>> GetExpressionSubpathesDict(this IEnumerable<IncludePath> pathes)
    {
        var result = GetExpressionSubpathes(pathes);

        return result
            .GroupBy(x => x.Key)
            .ToDictionary(gr => gr.Key, gr => gr.Select(x => x.Value));
    }

    public static IEnumerable<KeyValuePair<IncludeExpression, IncludePath>> GetExpressionSubpathes(this IEnumerable<IncludePath> pathes)
    {
        return pathes
            .Select(x => new {
                exp = IncludeExpression.Search(x.Root),
                sub = x.Subpath
            })
            .Where(z => z.exp != null && z.sub != null)
            .Select(x => new KeyValuePair<IncludeExpression, IncludePath>(x.exp!, x.sub!));
    }

    public static Dictionary<TKey, IEnumerable<TValue>> GroupToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull => 
        source.GroupBy(x => x.Key).ToDictionary(gr => gr.Key, gr => gr.Select(x => x.Value));
}

public static class UniversalGenericExtensions
{
    public static IEnumerable<KeyValuePair<TKeyNew, TValue>> NewKeys<TKeyOld, TKeyNew, TValue>(this IEnumerable<KeyValuePair<TKeyOld, TValue>> source, Func<TKeyOld, TKeyNew> change) =>
        source.Select(x => new KeyValuePair<TKeyNew, TValue>(change(x.Key), x.Value));
}