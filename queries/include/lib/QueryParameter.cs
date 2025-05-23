namespace Nist;

public record IncludeQueryParameter(CommaSeparatedParameters<ObjectPath> Inner) : CommaSeparatedParameters<ObjectPath>(Inner)
{
    public static bool TryParse(string source, out IncludeQueryParameter includeQueryParameter)
    {
        includeQueryParameter = Parse(source);
        return true;
    }

    public static IncludeQueryParameter Parse(string source) => new(
        Parse(source, x => ObjectPath.Parse(x))
    );

    public override string ToString() => base.ToString();
}