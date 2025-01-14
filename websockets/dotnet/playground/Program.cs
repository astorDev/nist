using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseWebSockets();

app.Map("/echo", async (HttpContext context) => {
    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[8];
    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Text, true, CancellationToken.None);
});

app.Run();