namespace Nist;

public record SortQueryParameter(CommaSeparatedParameters<SortField> Inner) : CommaSeparatedParameters<SortField>(Inner)
{
    public static bool TryParse(string source, out SortQueryParameter includeQueryParameter)
    {
        includeQueryParameter = Parse(source);
        return true;
    }

    public static SortQueryParameter Parse(string source)
    {
        return new SortQueryParameter(Parse(source, SortField.Parse));
    }

    public static SortQueryParameter Single(SortField field)
    {
        return new SortQueryParameter(new([field]));
    }
}

public record SortField(string FieldName, bool Ascending)
{
    public static SortField Descending(string field) => new(field, false);

    public static SortField Parse(string source)
    {
        return source.StartsWith('-') ?
            new SortField(source[1..], false) 
            : new SortField(source, true);
    }

    public override string ToString()
    {
        return (Ascending ? "" : "-") + FieldName;
    }
}