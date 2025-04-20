# Proxy HTTP Requests in .NET 9 Like a Pro

> A Simple Guide to Writing Your Own Reverse-Proxy using C# in 2025.

Proxying, or reverse-proxying, a request is a pretty common task, especially in a microservices architecture. One of the scenarios I bump into frequently is developing a gateway, authorizing a user, and proxying a request to a private microservice by an updated path. There's already a tool in the .NET ecosystem handling the task. It is called [YARP](https://github.com/dotnet/yarp/tree/main) - Yet Another Reverse Proxy. However, for me, it feels over-engineered. If you feel the same, welcome to the article, where we will create our own simple version, although inspired by YARP.

> If you don't want to implement it, but are still interested in getting a simple reverse-proxy, jump straight to the end of the article, to the [TLDR; section](#tldr).

## Building the Skeleton: Proxy Extension Method

- Uri - Passed to request as an argument.
- Http Method - From Request

```csharp
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
```

- Content
- Headers
- Status Code - From Response

```csharp
public static async Task CopyTo(this HttpResponseMessage source, HttpResponse target)
{
    target.StatusCode = (int)source.StatusCode;
    target.SetHeaders(source.AllHeaders());
    await target.CopyContentFrom(source.Content);
}
```

```csharp
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
```

## Copying Headers: The Peculiar .NET Structuring

```csharp
var client = new HttpClient() { BaseAddress = new Uri("https://api.spacexdata.com/v4") };
var response = await client.GetAsync("launches/latest");

Console.WriteLine("\n\n------Headers directly in response:-----\n\n");
foreach (var header in response.Headers)
    Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");

Console.WriteLine("\n\n------Headers in content:----------\n\n");
foreach (var header in response.Content.Headers)
    Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
```

```text
------Headers directly in response:-----
 
 
 Date: Sun, 20 Apr 2025 08:47:59 GMT
 Connection: keep-alive
 Access-Control-Allow-Origin: *
 Access-Control-Expose-Headers: spacex-api-cache,spacex-api-response-time
 Alt-Svc: h3=":443"
 Content-Security-Policy: default-src 'self';base-uri 'self';block-all-mixed-content;font-src 'self' https: data:;frame-ancestors 'self';img-src 'self' data:;object-src 'none';script-src 'self';script-src-attr 'none';style-src 'self' https: 'unsafe-inline';upgrade-insecure-requests
 Expect-CT: max-age=0
 Referrer-Policy: no-referrer
 Server: cloudflare
 Spacex-Api-Response-Time: 0ms
 Strict-Transport-Security: max-age=15552000; includeSubDomains
 Vary: Origin
 X-Content-Type-Options: nosniff
 X-Dns-Prefetch-Control: off
 X-Download-Options: noopen
 X-Frame-Options: SAMEORIGIN
 X-Permitted-Cross-Domain-Policies: none
 X-XSS-Protection: 0
 Cf-Cache-Status: DYNAMIC
 CF-RAY: 93336148cbe0d104-CDG
 
 
 ------Headers in content:----------
 
 
 Content-Type: text/plain; charset=utf-8
 Content-Length: 9
```

```csharp
public record HttpHeader(string Key, IEnumerable<string> Value)
{
    public static implicit operator HttpHeader(KeyValuePair<string, StringValues> header) => new(header.Key, header.Value);
}
```

```csharp
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
```

```csharp
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
```

## Proxying Content: Keep It Streamed

```csharp
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
```

## Testing The Proxy: Utilizing Postman Echo

```sh
dotnet new web
```

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<PostmanEchoClient>(client => {
    client.BaseAddress = new Uri("https://postman-echo.com");
});

var app = builder.Build();

app.MapPost("/echo", async (PostmanEchoClient echo, HttpContext context) => {
    await echo.Http.Proxy(context, "post");
});

app.Run();

public class PostmanEchoClient(HttpClient http) {
    public HttpClient Http => http;
}
```

```http
POST http://localhost:5155/echo

{
    "example" : "one"
}
```

```json
{
  "args": {},
  "data": {
    "example": "one"
  },
  "files": {},
  "form": {},
  "headers": {
    "host": "postman-echo.com",
    "x-request-start": "t1745139404.344",
    "connection": "close",
    "content-length": "25",
    "x-forwarded-proto": "https",
    "x-forwarded-port": "443",
    "x-amzn-trace-id": "Root=1-6804b6cc-17c6d05d291f9b32648d2035",
    "accept": "*/*",
    "user-agent": "httpyac",
    "accept-encoding": "gzip, deflate, br",
    "traceparent": "00-718835a3ad62bd56f53f966e83e1545a-dd3329f7582f0a2e-00",
    "content-type": "application/json"
  },
  "json": {
    "example": "one"
  },
  "url": "https://postman-echo.com/post"
}
```

## TLDR;

In this article, we've built a proxy extension method for `HttpClient`. Instead of implementing it again in your project, you can just install the package below:

```sh
dotnet add package Nist.Proxy
```

Here's the most basic test for the package:

> You can find the complete code [here on Github](https://github.com/astorDev/nist/blob/main/proxy/dotnet/playground/Program.cs). Note that `AddHttpService` is part of the `Nist.Registration` package, so you need to install it first. 

```csharp
builder.Services.AddHttpService<GithubClient>(new Uri("http://api.github.com"));

// ..

app.MapGet("/author", async (GithubClient github, HttpContext context) => {
    await github.Http.Proxy(context, "users/astorDev");
});

public class GithubClient(HttpClient http) {
    public HttpClient Http => http;
}
```

Executing `GET /author`, you should get basic info about the GitHub profile of me, the author of this article. 

The package, as well as this article, is part of the [NIST project](https://github.com/astorDev/nist). The project's purpose in a few words is to be a Non-Toxic REST alternative, so there's many interesting stuff beyond proxying - check it out and don't hesitate to give it a star! ⭐

Claps for this article are also highly appreciated! 😉
