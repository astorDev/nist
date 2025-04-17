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

        // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
        context.Response.Headers.Remove("transfer-encoding");
    }

    public static HttpRequestMessage CopyWith(this HttpRequest request, string? route = null)
    {
        var requestMessage = new HttpRequestMessage(new(request.Method), route);

        requestMessage.TryCopyContent(request);
        requestMessage.AddHeaders(request.Headers.Where(k => k.Key != "Host"));

        return requestMessage;
    }

    public static async Task CopyTo(this HttpResponseMessage responseMessage, HttpResponse response)
    {
        response.StatusCode = (int)responseMessage.StatusCode;
        response.SetHeaders(responseMessage.GetHeaders());
        await response.CopyContentFrom(responseMessage.Content);
    }

    public static async Task CopyContentFrom(this HttpResponse response, HttpContent content)
    {
        var stream = await content.ReadAsStreamAsync();
        await stream.CopyToAsync(response.Body);
    }

    public static void SetContent(this HttpRequestMessage requestMessage, Stream stream)
    {
        var streamContent = new StreamContent(stream);
        requestMessage.Content = streamContent;
    }

    public static bool HasBodylessMethod(this HttpRequest request)
    {
        var method = request.Method;

        return string.Equals(method, HttpMethods.Get, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(method, HttpMethods.Head, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(method, HttpMethods.Delete, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(method, HttpMethods.Trace, StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryCopyContent(this HttpRequestMessage requestMessage, HttpRequest request)
    {
        if (requestMessage.Content != null)
            return false;

        if (request.HasBodylessMethod())
            return false;

        var streamContent = new StreamContent(request.Body);
        requestMessage.Content = streamContent;
        return true;
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