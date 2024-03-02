using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Webapi.Core.Infrastructure
{
    public class EchoWebSocketHandler : IWebSocketHandler
    {
        Microsoft.Extensions.Logging.ILogger logger { get; set; }

        WebSocket _webSocket = null;
        public EchoWebSocketHandler(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(nameof(EchoWebSocketHandler));
        }

        public string Info { get; private set; }
        public Task CloseAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string closeStatusDescription = null, CancellationToken cancellationToken = default)
        {
            return _webSocket?.CloseAsync(closeStatus, closeStatusDescription, cancellationToken);
        }

        public void Dispose()
        {
            _webSocket?.Dispose();
            _webSocket = null;
        }

        public async Task HandleWebSocket(HttpContext context, WebSocket webSocket, WebSocketsConfig webSocketsConfig)
        {
            _webSocket = webSocket;
            var conn = context.Connection;
            this.Info = $"connectionId: {conn.Id}, local ep: {conn.LocalIpAddress.MapToIPv4()}:{conn.LocalPort}, remote ep: {conn.RemoteIpAddress.MapToIPv4()}:{conn.RemotePort}";
            var buff = new ArraySegment<byte>(new byte[webSocketsConfig.BufferSize]);
            var pack = await _webSocket.ReceiveAsync(buff, CancellationToken.None);
            while (pack.MessageType != WebSocketMessageType.Close)
            {
                await _webSocket.SendAsync(new ArraySegment<byte>(buff.Take(pack.Count).ToArray()), pack.MessageType, pack.EndOfMessage, CancellationToken.None);
            }
            await CloseAsync();
        }

        public async Task<bool> SendBinaryAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            try
            {
                await _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(SendBinaryAsync));
            }
            return false;
        }

        public async Task<bool> SendTextAsync(string data, CancellationToken cancellationToken = default)
        {
            try
            {
                await _webSocket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(SendTextAsync));
            }
            return false;
        }
    }

}