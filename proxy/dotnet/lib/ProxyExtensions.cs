using Microsoft.AspNetCore.Http;

namespace Nist;

public static class ProxyExtensions
{
    public static async Task Proxy(this HttpMessageInvoker invoker, HttpContext context, string? route = null, CancellationToken? cancellationToken = null)
    {
        var request = context.Request.ToProxyMessageWith(route);
        var response = await invoker.SendAsync(request, cancellationToken ?? CancellationToken.None);
        await response.CopyTo(context.Response);

        // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
        context.Response.Headers.Remove("transfer-encoding");
    }

    public static HttpRequestMessage ToProxyMessageWith(this HttpRequest source, string? route = null)
    {
        var target = new HttpRequestMessage(
            method: new(source.Method), 
            requestUri: route
        );

        target.AddHeaders(source.Headers.Except("Host"));
        target.Content = source.OptionalStreamContent();

        return target;
    }

    public static async Task CopyTo(this HttpResponseMessage source, HttpResponse target)
    {
        target.StatusCode = (int)source.StatusCode;
        target.SetHeaders(source.AllHeaders());
        await target.CopyContentFrom(source.Content);
    }
}