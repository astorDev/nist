using System.Collections;
using System.Net;

using Astor.Linq;

namespace Nist.Queries;

public record QueryUri(string Url, IEnumerable<QueryKeyValue> QueryParams)
{
    public static QueryUri From(string url, object queryObject) => new(url, GetQueryKeyValues(queryObject));

    public static IEnumerable<QueryKeyValue> GetQueryKeyValues(object queryObject)
    {
        static string CamelCased(string source) => source[0].ToString().ToLower() + new string(source.Skip(1).ToArray());

        var props = queryObject.GetType().GetProperties().Select(p => new { Name = CamelCased(p.Name), Value = p.GetValue(queryObject) });
        var notNullProps = props.Where(p => p.Value != null);
        
        var (enumerableProps, flatProps) = notNullProps.Fork(p => p.Value is IEnumerable && p.Value is not string);
        var (dictionaryProps, standardEnumerableProps) = enumerableProps.Fork(p => p.Value is IDictionary);

        foreach (var dictionaryProp in dictionaryProps) 
        {
            IDictionary dictionary = (IDictionary)dictionaryProp.Value!;
            foreach (var key in ((IDictionary)dictionaryProp.Value!).Keys) {
                var keyValue = dictionary[key]!;
                yield return QueryKeyValue.From($"{dictionaryProp.Name}.{key}", keyValue);
            }
        }

        foreach (var prop in standardEnumerableProps)
        {
            foreach (var v in (IEnumerable) prop.Value!) yield return QueryKeyValue.From(prop.Name, v);
        }

        foreach (var prop in flatProps) yield return QueryKeyValue.From(prop.Name, prop.Value!);
    }

    public static implicit operator string(QueryUri uri) => uri.ToString();
    public override string ToString() => this.Url + (this.QueryParams.Any() ? "?" + String.Join("&", this.QueryParams) : String.Empty);
}

public record QueryKeyValue(string Key, string Value)
{
    public override string ToString() => $"{this.Key}={this.Value}";

    public static QueryKeyValue From(string key, object value) => value switch
    {
        DateTime time => new(key, time.ToUniversalTime().ToString("O")),
        _ => new(key, WebUtility.UrlEncode(value.ToString())!)
    };
}

