using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Caching;

namespace Webapi.Data.Caching
{
    public class EmptyCacheManager : CacheKeyService, ILocker, IStaticCacheManager
    {
        public EmptyCacheManager(AppSettings appSettings) : base(appSettings)
        {

        }

        public Task ClearAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }

        public T Get<T>(CacheKey key, Func<T> acquire)
        {
            return acquire == null ? default : acquire.Invoke();
        }

        public async Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire)
        {
            return acquire == null ? default : await acquire?.Invoke();
        }

        public Task RemoveAsync(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
        {
            return Task.CompletedTask;
        }

        public Task SetAsync(CacheKey key, object data)
        {
            return Task.CompletedTask;
        }

        ConcurrentDictionary<string, Action> Locks { get; } = new ConcurrentDictionary<string, Action>();

        public bool PerformActionWithLock(string resource, TimeSpan expirationTime, Action action)
        {
            var succ = Locks.TryAdd(resource, action);
            if (succ)
            {
                var cancellSource = new CancellationTokenSource();
                Task.Delay(expirationTime, cancellSource.Token).ContinueWith(_ => Locks.TryRemove(resource, out var _));
                try
                {
                    action();
                }
                finally
                {
                    cancellSource.Cancel();
                }
            }
            return succ;
        }
    }
}
