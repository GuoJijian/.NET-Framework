using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Utils;

namespace Webapi.Data.Caching
{
    public class RedisDistributedCache : RedisCache, IRedisDistributedCache
    {
        RedisCacheOptions Options { get; }
        public RedisDistributedCache(IOptions<RedisCacheOptions> optionsAccessor) : base(optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public IDatabase Default => this.Private<RedisCache, IDatabase>("_cache");
        public IConnectionMultiplexer Connection => this.Private<RedisCache, IConnectionMultiplexer>("_connection");


        public async IAsyncEnumerable<string> Keys(string pattern = null, int dbId = -1)
        {
            if (Default == null)
            {
                await GetAsync("test");
            }
            var redisConnectOptions = ConfigurationOptions.Parse(Options.Configuration);
            dbId = dbId >= 0 ? dbId : redisConnectOptions.DefaultDatabase.HasValue ? redisConnectOptions.DefaultDatabase.Value : -1;
            var redisServer = Connection?.GetServer(redisConnectOptions.EndPoints.First());

            if (redisServer != null)
            {
                await foreach (var key in redisServer.KeysAsync(dbId, (pattern ?? string.Empty) + "*"))
                {
                    yield return key;
                }
            }
        }

        public async Task RemoveByPrefixAsync(string prefix, int dbId = -1)
        {
            var keys = await Keys(prefix, dbId).ToArrayAsync();
            var redisdb = dbId == -1 ? Default : Connection.GetDatabase(dbId);
            await redisdb.KeyDeleteAsync(keys.Select(p => new RedisKey(p)).ToArray());
        }
    }
}
