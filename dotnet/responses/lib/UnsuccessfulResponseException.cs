using System.Net;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace Nist.Responses;

public class UnsuccessfulResponseException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public HttpResponseHeaders Headers { get; }
    public string Body { get; }

    public override string Message => $"{StatusCode} status code received";

    public UnsuccessfulResponseException(HttpStatusCode statusCode, HttpResponseHeaders headers, string body)
    {
        StatusCode = statusCode;
        Headers = headers;
        Body = body;
    }

    public T? DeserializedBody<T>()
    {
        return JsonConvert.DeserializeObject<T>(this.Body);
    }
}