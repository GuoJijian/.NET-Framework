using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Webapi.Core
{
    public interface IRedisDistributedCache : IDistributedCache
    {
        IAsyncEnumerable<string> Keys(string pattern = null, int dbId = -1);

        Task RemoveByPrefixAsync(string prefix, int dbId = -1);
        IConnectionMultiplexer Connection { get; }
        IDatabase Default { get; }
    }
}
