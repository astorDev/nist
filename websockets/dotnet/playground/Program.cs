using System.Net.WebSockets;
using System.Text;
using Nist;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);

var app = builder.Build();

app.UseWebSockets();

app.Map("/echo", async (HttpContext context) => {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

    var buffer = new byte[8];

    await webSocket.ReceiveAsync(buffer, CancellationToken.None);

    await webSocket.SendAsync(
        buffer, 
        WebSocketMessageType.Text, 
        endOfMessage: true, 
        CancellationToken.None
    );
});

app.Map("/echo-long", async (HttpContext context) => {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = await webSocket.ReceiveToTheEndAsync();
    await webSocket.SendFullTextAsync(buffer);
});

app.Map("/echo-forever", async (HttpContext context, CancellationToken cancellationToken) => {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

    while (!cancellationToken.IsCancellationRequested)
    {
        var buffer = await webSocket.ReceiveToTheEndAsync();
        await webSocket.SendFullTextAsync(buffer);    
    }
});

app.Map("/echo-forever-unexceptioned", async (HttpContext context, CancellationToken cancellationToken) => {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

    while (!cancellationToken.IsCancellationRequested && webSocket.State == WebSocketState.Open)
    {
        var buffer = await webSocket.ReceiveToTheEndAsync(cancellationToken);
        app.Logger.LogInformation("Received: {buffer}", Encoding.UTF8.GetString(buffer));
        await webSocket.SendFullTextAsync(buffer, cancellationToken);
    }

    app.Logger.LogInformation("WebSocket closed");
});

app.Map("/echo-forever-right", async (HttpContext context, CancellationToken cancellationToken) => {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

    while (!cancellationToken.IsCancellationRequested && webSocket.State == WebSocketState.Open)
    {
        var buffer = await webSocket.ReceiveToTheEndAsync(cancellationToken);
        app.Logger.LogInformation("Received: {buffer}", Encoding.UTF8.GetString(buffer));
        await webSocket.SendFullTextAsync(buffer, cancellationToken);
    }

    if (webSocket.State == WebSocketState.CloseReceived)
    {
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
    }

    app.Logger.LogInformation("WebSocket closed");
});

app.Map("/echo-forever-final", async (HttpContext context, CancellationToken cancellationToken) => {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

    while (!cancellationToken.IsCancellationRequested && webSocket.State == WebSocketState.Open)
    {
        var buffer = await webSocket.ReceiveAsync(cancellationToken: cancellationToken);
        app.Logger.LogInformation("Received: {buffer}", Encoding.UTF8.GetString(buffer));
        await webSocket.SendAsync(buffer, cancellationToken: cancellationToken);
    }

    if (webSocket.State == WebSocketState.CloseReceived)
    {
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
    }

    app.Logger.LogInformation("WebSocket closed");
});

app.Run();

public static class WebSocketExtensions
{
    public static async Task<byte[]> ReceiveToTheEndAsync(this WebSocket webSocket, CancellationToken? cancellationToken = null)
    {
        var buffer = new byte[8];
        using var memoryStream = new MemoryStream();
        WebSocketReceiveResult result;

        do
        {
            result = await webSocket.ReceiveAsync(buffer, cancellationToken ?? CancellationToken.None);
            memoryStream.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        return memoryStream.ToArray();
    }

    public static async Task SendFullTextAsync(this WebSocket webSocket, ArraySegment<byte> buffer, CancellationToken? cancellationToken = null)
    {
        await webSocket.SendAsync(
            buffer,
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken ?? CancellationToken.None
        );
    }
}