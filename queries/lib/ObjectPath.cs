namespace Nist;

public record ObjectPath(
    string Root,
    ObjectPath? Child
)
{
    public static ObjectPath Parse(string path, string separator = ".")
    {
        var parts = path.Split(separator);
        return Parse(parts);
    }

    public static ObjectPath Parse(string[] parts) => new (
        parts[0],
        parts.Length > 1 ? Parse(parts[1..]) : null
    );

    override public string ToString()
    {
        return Child != null ? $"{Root}.{Child}" : Root;
    }
}