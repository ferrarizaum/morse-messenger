using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// aspnetcore lib to handle websockets
app.UseWebSockets();

app.Map("/ws", async context =>
{
    // listening for a websocket request
    if (context.WebSockets.IsWebSocketRequest)
    {
        // if websocket request, we open connection
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("Client connected!");

        await HandleWebSocket(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

async Task HandleWebSocket(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4]; // 4KB size, 1 KB === 1024 bytes
    while (webSocket.State == WebSocketState.Open)
    {
        // while websocket connection open, we listen for messages
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Text)
        {
            // if message type === text, we print it
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received: {message}");

            var response = Encoding.UTF8.GetBytes($"Echo: {message}");
            await webSocket.SendAsync(
                new ArraySegment<byte>(response),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
        else if (result.MessageType == WebSocketMessageType.Close)
        {
            Console.WriteLine("Client disconnected");
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            break;
        }
    }
}

await app.RunAsync();