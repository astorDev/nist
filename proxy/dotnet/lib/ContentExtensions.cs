using Microsoft.AspNetCore.Http;

namespace Nist;

public static class ContentExtensions
{
    public static StreamContent? OptionalStreamContent(this HttpRequest request)
    {
        return request.ContentLength > 0 ? new StreamContent(request.Body) : null;
    }

    public static async Task CopyContentFrom(this HttpResponse response, HttpContent content)
    {
        using var stream = await content.ReadAsStreamAsync();
        await stream.CopyToAsync(response.Body);
    }
}