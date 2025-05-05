using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Nist;

public class DictionaryQueryParameters : Dictionary<string, string>
{
    public DictionaryQueryParameters(IDictionary<string, object> source) : this(
        source.ToDictionary(kv => kv.Key, kv => QueryStringValue.From(kv.Value))
    )
    {
    }

    public DictionaryQueryParameters(IDictionary<string, string> source) : base(source)
    {
    }

    public static ValueTask<DictionaryQueryParameters?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    {
        var queryParsed = httpContext.Request.Query.Select(kvp => {
            ObjectPath path = ObjectPath.Parse(kvp.Key);
            
            return new {
                Key = path.Root,
                Child = path.Child?.Root,
                Grandchild = path.Child?.Child?.Root,
                Value = kvp.Value.ToString()
            };
        });

        var matchingQueryParameters = queryParsed.Where(kvp => 
            kvp.Key.Equals(parameter.Name, StringComparison.InvariantCultureIgnoreCase) 
            && kvp.Child != null 
            && kvp.Grandchild == null
        );

        var result = matchingQueryParameters.ToDictionary(
            kvp => kvp.Child!, 
            kvp => kvp.Value
        );

        return ValueTask.FromResult<DictionaryQueryParameters?>(new (result));
    }
}