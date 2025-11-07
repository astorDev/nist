using System.Collections;

namespace Nist;

public class NistException(Error error, Dictionary<string, object?>? details = null) : Exception(error.Reason)
{
    public override IDictionary Data => details ?? [];
}

public record CommaSeparatedStringsParameter(CommaSeparatedParameters<string> Inner) : CommaSeparatedParameters<string>(Inner)
{
    public static bool TryParse(string source, out CommaSeparatedStringsParameter stringCommaSeparatedParameters)
    {
        stringCommaSeparatedParameters = new CommaSeparatedStringsParameter(Parse(source, x => x));
        return true;
    }
}

public static class EnumerableExtensions
{
    public static TTarget[] Map<TOriginal, TTarget>(this IEnumerable<TOriginal> source, Func<TOriginal, TTarget> mapper) => [.. source.Select(mapper)];
}