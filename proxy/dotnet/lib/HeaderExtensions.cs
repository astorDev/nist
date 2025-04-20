using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Nist;

public record HttpHeader(string Key, IEnumerable<string> Value)
{
    public static implicit operator HttpHeader(KeyValuePair<string, StringValues> header) => new(header.Key, header.Value);
}

public static class RequestHeaderExtensions
{
    public static IEnumerable<HttpHeader> Except(this IHeaderDictionary headers, string headerKey)
    {
        return headers
            .Where(x => x.Key != headerKey)
            .Select(x => new HttpHeader(x.Key, x.Value));
    }

    public static void AddHeaders(this HttpRequestMessage requestMessage, IEnumerable<HttpHeader> headers)
    {
        foreach (var header in headers)
        {
            requestMessage.AddHeader(header);
        }
    }

    public static HttpHeader? AddHeader(this HttpRequestMessage request, HttpHeader header)
    {
        return request.AddRequestHeader(header) ?? request.AddContentHeader(header);
    }

    public static HttpHeader? AddRequestHeader(this HttpRequestMessage request, HttpHeader candidate)
    {
        var success = request.Headers.TryAddWithoutValidation(candidate.Key, candidate.Value);
        return success ? candidate : null;
    }

    public static HttpHeader? AddContentHeader(this HttpRequestMessage request, HttpHeader candidate)
    {
        var success = request.Content?.Headers.TryAddWithoutValidation(candidate.Key, candidate.Value) ?? false;
        return success ? new HttpHeader(candidate.Key, candidate.Value) : null;
    }
}

public static class ResponseHeaderExtensions
{
    public static IEnumerable<HttpHeader> AllHeaders(this HttpResponseMessage response)
    {
        return response.Headers.Concat(response.Content.Headers)
            .Select(x => new HttpHeader(x.Key, x.Value));
    }

    public static void SetHeaders(this HttpResponse response, IEnumerable<HttpHeader> headers)
    {
        foreach (var header in headers)
        {
            response.Headers[header.Key] = header.Value.ToArray();
        }
    }
}

