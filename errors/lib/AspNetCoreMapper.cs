using System.Net;
using System.Text.Json;

namespace Nist;

public static class AspNetCoreErrors
{
    public static Error RequestSchemaMismatch(JsonException jsonException) => new(HttpStatusCode.BadRequest, "RequestSchemaMismatch")
    {
        Data = new Dictionary<string, object?>
        {
            [ "path" ] = jsonException.Path
        }
    };

    public static Error? SearchAspNetCoreErrors(this Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException badHttpRequestException => badHttpRequestException.SearchError(),
            _ => null
        };
    }

    public static Error? SearchError(this BadHttpRequestException exception)
    {
        if (exception.InnerException is JsonException jsonException)
        {
            return RequestSchemaMismatch(jsonException);
        }

        return null;
    }
}