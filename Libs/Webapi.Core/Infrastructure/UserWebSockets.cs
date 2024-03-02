using System.Collections.Concurrent;

namespace Webapi.Core.Infrastructure
{
    public class UserWebSockets : ConcurrentDictionary<uint, IForwardWebSocketHandler>, IUserWebsockets
    {

    }
}