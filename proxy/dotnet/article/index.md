# Proxy HTTP Requests in .NET 9 Like a Pro

> A Simple Guide to Writing Your Own Reverse-Proxy using C# in 2025.

Proxying, or reverse-proxying, request is a pretty common task, especially in a microservices architecture. One of the scenario I bump into frequently is developing a gateway, authorizing a user and proxying request to a private microservice by an updated path. There's already a tool in a .NET ecosystem handling the task. It is called [YARP](https://github.com/dotnet/yarp/tree/main) - Yet Another Reverse Proxy. However, for me it feels over-engineered. If you feel the same welcome to the article when we will create our own simple version, although inspired by YARP.

> If you don't want to implement it, but still interested in getting a simple reverse-proxy, jump straight to the end of the article, to the [TLDR; section](#tldr).

## TLDR;

In this article, we've build a proxy extension method for `HttpClient`. Instead of implementing it again in your project you can just install the package below:

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

Executing `GET /author` you should get basic info about my, the author of this article, GitHub profile. 

The package, as well as this article, is part of the [NIST project](https://github.com/astorDev/nist). The project's purpose in a few words is to be a Non-Toxic REST alternative, so there's many interesting stuff beyond proxying - check it out and don't hesitate to give it a star! ⭐

Claps for this article are also highly appreciated! 😉