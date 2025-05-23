using System.Collections;

namespace Nist;

public record CommaSeparatedParameters<T>(ICollection<T> parameters) : IEnumerable<T>, IQueryParameter
    where T : notnull
{
    public CommaSeparatedParameters() : this(new List<T>()) { }

    public static CommaSeparatedParameters<T> Parse(string value, Func<string, T> parameterParser)
    {
        var rawParameters = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var parameters = rawParameters.Select(parameterParser).ToList();
        return [.. parameters];
    }

    public void Add(T parameter) => parameters.Add(parameter);

    public IEnumerator<T> GetEnumerator() => parameters.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => string.Join(',', parameters);
}

public record CommaSeparatedStringParameters(ICollection<string> parameters) : CommaSeparatedParameters<string>(parameters)
{
    public static CommaSeparatedStringParameters Parse(string value)
    {
        var rawParameters = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var parameters = rawParameters.Select(v => v.Trim()).Where(v => !string.IsNullOrEmpty(v)).ToArray();
        return new CommaSeparatedStringParameters(parameters);
    }
}