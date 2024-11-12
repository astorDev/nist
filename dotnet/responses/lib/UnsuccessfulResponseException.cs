using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Nist.Responses;

public class UnsuccessfulResponseException(HttpStatusCode statusCode, HttpResponseHeaders headers, string body) : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public HttpResponseHeaders Headers { get; } = headers;
    public string Body { get; } = body;

    public override string Message => $"{StatusCode} status code received";

    public T? DeserializedBody<T>(JsonSerializerOptions? serializerOptions = null)
    {
        return JsonSerializer.Deserialize<T>(this.Body, serializerOptions);
    }
}