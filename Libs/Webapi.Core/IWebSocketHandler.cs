using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Webapi.Core
{
    public interface IWebSocketHandler : IDisposable
    {
        Task HandleWebSocket(HttpContext context, WebSocket webSocket, WebSocketsConfig webSocketsConfig);
        Task<bool> SendTextAsync(string data, CancellationToken cancellationToken = default);
        Task<bool> SendBinaryAsync(byte[] data, CancellationToken cancellationToken = default);
        Task CloseAsync(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string closeStatusDescription = null, CancellationToken cancellationToken = default);
    }

    public interface IForwardWebSocketHandler : IWebSocketHandler
    {
        uint TempId { get; }        
    }

    public interface IUserWebsockets : IReadOnlyDictionary<uint, IForwardWebSocketHandler>
    {
        bool TryRemove(uint id, out IForwardWebSocketHandler webSocketHandler);
        bool TryAdd(uint id, IForwardWebSocketHandler webSocketHandler);
    }
}
