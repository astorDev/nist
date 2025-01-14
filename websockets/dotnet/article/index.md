# WebSockets in .NET 9: A Getting Started Guide

> Learn how to implement WebSocket server endpoints using C# with ASP .NET Core 9 Minimal API.

WebSockets are the simplest way to provide real-time communication between front-end and back-end apps. And it's absolutely possible to use `ASP .NET Core` for that style of communication. However, the Microsoft's  documentation for that technology seems poor and almost abandoned. In this article, I'm going to try fill the gap, giving you the jump start with WebSockets using the latest `.NET` stack.

> Or jump straight to the end for the [TLDR](#the-final-version)

## The Naive Approach: One-Time Echo

```sh
dotnet new web --name Playground.WebSockets
```

## The Final Version

There's a dedicated nuget package containing extension methods. Let's install it:

```sh
dotnet add package Nist.WebSockets
```

Here's how our echo endpoint will look after utilizing the package:

```csharp
app.Map("/echo-forever-final", async (HttpContext context, CancellationToken cancellationToken) => {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

    while (!cancellationToken.IsCancellationRequested && webSocket.State == WebSocketState.Open)
    {
        var buffer = await webSocket.ReceiveAsync(cancellationToken);
        app.Logger.LogInformation("Received: {buffer}", Encoding.UTF8.GetString(buffer));
        await webSocket.SendAsync(buffer, cancellationToken);
    }

    if (webSocket.State == WebSocketState.CloseReceived)
    {
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
    }

    app.Logger.LogInformation("WebSocket closed");
});
```

The package is part of the [Nist](https://github.com/astorDev/nist) project, providing tools for **Ni**ce **S**tate **T**ransfer. In the repository you can find the [article source code](https://github.com/astorDev/nist/tree/main/websockets/dotnet/playground) as well as the [package source code](https://github.com/astorDev/nist/tree/main/websockets/dotnet/lib). Give it a Star on the [Github](https://github.com/astorDev/nist)! And also ... claps for the article are appreciated 👉👈
