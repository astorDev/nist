using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Nist;

public static class ProxyExtensions
{
    public static async Task Proxy(this HttpMessageInvoker invoker, HttpContext context, string? route = null, CancellationToken? cancellationToken = null)
    {
        var request = context.Request.CopyWith(route);
        var response = await invoker.SendAsync(request, cancellationToken ?? CancellationToken.None);
        await response.CopyTo(context.Response);
    }

    public static HttpRequestMessage CopyWith(this HttpRequest request, string? route = null)
    {
        var requestMessage = new HttpRequestMessage(new(request.Method), route);
        if (!request.HasBodylessMethod())
        {
            var streamContent = new StreamContent(request.Body);
            requestMessage.Content = streamContent;
        }

        requestMessage.AddHeaders(request.Headers.Where(k => k.Key != "Host"));

        return requestMessage;
    }

    public static async Task CopyTo(this HttpResponseMessage responseMessage, HttpResponse response)
    {
        response.StatusCode = (int)responseMessage.StatusCode;
        response.SetHeaders(responseMessage.GetHeaders());

        // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
        response.Headers.Remove("transfer-encoding");

        await using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
        await responseStream.CopyToAsync(response.Body);
    }

    public static bool HasBodylessMethod(this HttpRequest request)
    {
        var method = request.Method;

        return string.Equals(method, HttpMethods.Get, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(method, HttpMethods.Head, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(method, HttpMethods.Delete, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(method, HttpMethods.Trace, StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<KeyValuePair<string, IEnumerable<string>>> GetHeaders(this HttpResponseMessage response)
    {
        var rootHeaders = response.Headers
            .Select(rh => new KeyValuePair<string, IEnumerable<string>>(rh.Key, rh.Value));

        var contentHeaders = response.Content.Headers
                .Select(rh => new KeyValuePair<string, IEnumerable<string>>(rh.Key, rh.Value));

        return rootHeaders.Concat(contentHeaders);
    }

    public static void SetHeaders(this HttpResponse response, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        foreach (var header in headers)
        {
            response.Headers[header.Key] = header.Value.ToArray();
        }
    }

    public static void AddHeaders(this HttpRequestMessage requestMessage, IEnumerable<KeyValuePair<string, StringValues>> headers)
    {
        foreach (var header in headers)
        {
            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
    }
}