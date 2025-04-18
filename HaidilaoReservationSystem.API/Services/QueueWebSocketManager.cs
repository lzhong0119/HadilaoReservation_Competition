// Services/QueueWebSocketManager.cs
using HaidilaoReservationSystem.API.Data;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace HaidilaoReservationSystem.API.Services
{
    public class QueueWebSocketManager
    {
        private readonly List<WebSocket> _sockets = new();
        private readonly AppDbContext _context;
        private readonly ILogger<QueueWebSocketManager> _logger;

        public QueueWebSocketManager(AppDbContext context, ILogger<QueueWebSocketManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task HandleConnection(WebSocket webSocket)
        {
            _sockets.Add(webSocket);
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                        break;
                    }

                    // Handle incoming message if needed
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogInformation("Received WebSocket message: {Message}", message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket connection error");
            }
            finally
            {
                _sockets.Remove(webSocket);
            }
        }

        public async Task BroadcastQueueUpdate(int outletId)
        {
            var waitingCount = await _context.Queues
                .CountAsync(q => q.OutletId == outletId && q.Status == "Waiting");

            var message = new
            {
                type = "queueUpdate",
                outletId,
                waitingCount
            };

            var jsonMessage = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(jsonMessage);

            foreach (var socket in _sockets.Where(s => s.State == WebSocketState.Open))
            {
                try
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(buffer, 0, buffer.Length),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending WebSocket message");
                }
            }
        }
    }
}
