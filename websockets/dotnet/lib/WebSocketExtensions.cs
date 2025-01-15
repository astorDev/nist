using System.Net.WebSockets;

namespace Nist;

public static class WebSocketExtensions
{
    public static async Task<byte[]> ReceiveAsync(this WebSocket webSocket, CancellationToken? cancellationToken = null)
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

    public static async Task SendAsync(this WebSocket webSocket, ArraySegment<byte> buffer, WebSocketMessageType messageType = WebSocketMessageType.Text, bool endOfMessage = true, CancellationToken? cancellationToken = null)
    {
        await webSocket.SendAsync(
            buffer,
            messageType,
            endOfMessage,
            cancellationToken ?? CancellationToken.None
        );
    }
}
