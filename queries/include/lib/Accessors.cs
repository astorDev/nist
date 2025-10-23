using Nist;

public static class IncludeQueryParameterExtensions
{
    public static IEnumerable<ObjectPath> GetChildren(this IEnumerable<ObjectPath> pathes, string key)
    {
        return from p in pathes
               where p.Root == key && p.Child != null
               select p.Child;
    }

    public static bool Have(this IEnumerable<ObjectPath> pathes, string key)
    {
        return pathes.Any(p => p.Root == key);
    }

    public static bool Have(this IEnumerable<ObjectPath> paths, string segment1, string segment2)
    {
        if (!paths.Have(segment1))
            return false;

        var children = paths.GetChildren(segment1);
        return children.Have(segment2);
    }

    public static bool Have(this IEnumerable<ObjectPath> paths, string segment1, string segment2, string segment3)
    {
        if (!paths.Have(segment1))
            return false;

        var children = paths.GetChildren(segment1);
        return children.Have(segment2, segment3);
    }
}

public interface IQueryWithInclude
{
    IncludeQueryParameter? Include { get; }
}

public static class QueryWithIncludeHelper
{
    public static bool Includes(this IQueryWithInclude query, string value) =>
        query.Include?.Have(value) ?? false;

    public static bool Includes(this IQueryWithInclude query, string segment1, string segment2) =>
        query.Include?.Have(segment1, segment2) ?? false;

    public static bool Includes(this IQueryWithInclude query, string segment1, string segment2, string segment3) =>
        query.Include?.Have(segment1, segment2, segment3) ?? false;
}