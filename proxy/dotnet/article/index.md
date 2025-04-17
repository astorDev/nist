# Proxy HTTP Requests in .NET 9 Like a Pro

> A Simple Guide to Writing Your Own Reverse-Proxy using C# in 2025.

Proxying, or reverse-proxying, a request is a pretty common task, especially in a microservices architecture. One of the scenarios I bump into frequently is developing a gateway, authorizing a user, and proxying a request to a private microservice by an updated path. There's already a tool in the .NET ecosystem handling the task. It is called [YARP](https://github.com/dotnet/yarp/tree/main) - Yet Another Reverse Proxy. However, for me, it feels over-engineered. If you feel the same, welcome to the article, where we will create our own simple version, although inspired by YARP.

> If you don't want to implement it, but are still interested in getting a simple reverse-proxy, jump straight to the end of the article, to the [TLDR; section](#tldr).

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

```csharp
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
```

## Copying Request Content: Keep It Streamed

```csharp
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
```

## Copying Response Content: Keep It Streamed

```csharp
public static async Task CopyContentFrom(this HttpResponse response, HttpContent content)
{
    var stream = await content.ReadAsStreamAsync();
    await stream.CopyToAsync(response.Body);
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
