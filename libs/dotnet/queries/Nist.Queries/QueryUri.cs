using System.Collections;
using Astor.Linq;

namespace Nist.Queries;

public record QueryUri(string Url, IEnumerable<QueryKeyValue> QueryParams)
{
    public override string ToString()
    {
        return this.Url + (this.QueryParams.Any() ? "?" + String.Join("&", this.QueryParams) : String.Empty);
    }

    public static implicit operator string(QueryUri uri) => uri.ToString();

    public static QueryUri From(string url, object queryObject)
    {
        return new QueryUri(url, GetQueryKeyValues(queryObject));
    }

    public static IEnumerable<QueryKeyValue> GetQueryKeyValues(object queryObject)
    {
        var props = queryObject.GetType().GetProperties().Select(p => new { Name = CamelCased(p.Name), Value = p.GetValue(queryObject) });
        var notNullProps = props.Where(p => p.Value != null);
    
        var (enumerableProps, flatProps) = notNullProps.Fork(p => p.Value is IEnumerable && p.Value is not string);
 
        foreach (var prop in enumerableProps)
        {
            foreach (var v in (IEnumerable) prop.Value!)
            {
                yield return QueryKeyValue.From(prop.Name, v);
            }
        }

        foreach (var prop in flatProps)
        {
            yield return QueryKeyValue.From(prop.Name, prop.Value!);
        }
    }

    private static string CamelCased(string source)
    {
        return source[0].ToString().ToLower() + new string(source.Skip(1).ToArray());
    }
}
