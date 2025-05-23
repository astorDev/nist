using System.Collections;

namespace Nist;

public class IncludeQueryParameter(
    ObjectPath[] paths
) : IEnumerable<ObjectPath>
{
    public static bool TryParse(string source, out IncludeQueryParameter includeQueryParameter)
    {
        includeQueryParameter = Parse(source);
        return true;
    }

    public static IncludeQueryParameter Parse(string source)
    {
        var rawValues = source.Split(",");
        var parsedPathes = rawValues.Select(x => ObjectPath.Parse(x));
        return new IncludeQueryParameter([.. parsedPathes]);
    }

    public override string ToString()
    {
        return string.Join(",", paths.Select(p => p.ToString()));
    }

    public IEnumerator<ObjectPath> GetEnumerator()
    {
        return ((IEnumerable<ObjectPath>)paths).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}