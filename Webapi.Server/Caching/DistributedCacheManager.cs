using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Caching;
using Webapi.Core.ComponentModel;

namespace Webapi.Data.Caching
{
    /// <summary>
    /// Represents a distributed cache 
    /// </summary>
    public partial class DistributedCacheManager : CacheKeyService, ILocker, IStaticCacheManager
    {
        #region Fields

        private readonly IRedisDistributedCache _distributedCache;
        private readonly PerRequestCache _perRequestCache;

        #endregion

        #region Ctor

        public DistributedCacheManager(AppSettings appSettings, IRedisDistributedCache distributedCache, IHttpContextAccessor httpContextAccessor) : base(appSettings)
        {
            _distributedCache = distributedCache;
            _perRequestCache = new PerRequestCache(httpContextAccessor);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare cache entry options for the passed key
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>Cache entry options</returns>
        private DistributedCacheEntryOptions PrepareEntryOptions(CacheKey key)
        {
            //set expiration time for the passed cache key
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(key.CacheTime)
            };

            return options;
        }

        /// <summary>
        /// Try to get the cached item
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the flag which indicate is the key exists in the cache, cached item or default value
        /// </returns>
        private async Task<(bool isSet, T item)> TryGetItemAsync<T>(CacheKey key)
        {
            var json = await _distributedCache.GetStringAsync(key.Key);
            var (succ, item) = GetObject<T>(json);
            if (!succ)
            {
                return (false, item);
            }
            _perRequestCache.Set(key.Key, item);
            return (true, item);
        }

        /// <summary>
        /// Try to get the cached item
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Flag which indicate is the key exists in the cache, cached item or default value</returns>
        private (bool isSet, T item) TryGetItem<T>(CacheKey key)
        {
            var json = _distributedCache.GetString(key.Key);

            var (succ, item) = GetObject<T>(json);
            if (!succ)
            {
                return (false, item);
            }
            _perRequestCache.Set(key.Key, item);

            return (true, item);
        }

        /// <summary>
        /// Add the specified key and object to the cache
        /// </summary>
        /// <param name="key">Key of cached item</param>
        /// <param name="data">Value for caching</param>
        private void Set(CacheKey key, object data)
        {
            if ((key?.CacheTime ?? 0) <= 0 || data == null)
                return;

            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            _distributedCache.SetString(key.Key, JsonConvert.SerializeObject(GetDataTypeInfo(data.GetType(), settings), settings), PrepareEntryOptions(key));
            _perRequestCache.Set(key.Key, data);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the cached value associated with the specified key
        /// </returns>
        public async Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire)
        {
            //little performance workaround here:
            //we use "PerRequestCache" to cache a loaded object in memory for the current HTTP request.
            //this way we won't connect to Redis server many times per HTTP request (e.g. each time to load a locale or setting)
            if (_perRequestCache.IsSet(key.Key))
                return _perRequestCache.Get(key.Key, () => default(T));

            if (key.CacheTime <= 0)
                return acquire == null ? default : await acquire.Invoke();

            var (isSet, item) = await TryGetItemAsync<T>(key);

            if (isSet)
                return item;

            var result = acquire == null ? default : await acquire.Invoke();

            if (result != null)
                await SetAsync(key, result);

            return result;
        }

        /// <summary>
        /// Get a cached item. If it's not in the cache yet, then load and cache it
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="acquire">Function to load item if it's not in the cache yet</param>
        /// <returns>The cached value associated with the specified key</returns>
        public T Get<T>(CacheKey key, Func<T> acquire)
        {
            //little performance workaround here:
            //we use "PerRequestCache" to cache a loaded object in memory for the current HTTP request.
            //this way we won't connect to Redis server many times per HTTP request (e.g. each time to load a locale or setting)
            if (_perRequestCache.IsSet(key.Key))
                return _perRequestCache.Get(key.Key, () => default(T));

            if (key.CacheTime <= 0)
                return acquire == null ? default : acquire.Invoke();

            var (isSet, item) = TryGetItem<T>(key);

            if (isSet)
                return item;

            var result = acquire == null ? default : acquire.Invoke();

            if (result != null)
                Set(key, result);

            return result;
        }

        /// <summary>
        /// Remove the value with the specified key from the cache
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheKeyParameters">Parameters to create cache key</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task RemoveAsync(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            cacheKey = PrepareKey(cacheKey, cacheKeyParameters);

            await _distributedCache.RemoveAsync(cacheKey.Key);
            _perRequestCache.Remove(cacheKey.Key);
        }

        /// <summary>
        /// Add the specified key and object to the cache
        /// </summary>
        /// <param name="key">Key of cached item</param>
        /// <param name="data">Value for caching</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SetAsync(CacheKey key, object data)
        {
            if ((key?.CacheTime ?? 0) <= 0 || data == null)
                return;
            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            await _distributedCache.SetStringAsync(key.Key, JsonConvert.SerializeObject(GetDataTypeInfo(data, settings), settings), PrepareEntryOptions(key));
            _perRequestCache.Set(key.Key, data);
        }

        /// <summary>
        /// Remove items by cache key prefix
        /// </summary>
        /// <param name="prefix">Cache key prefix</param>
        /// <param name="prefixParameters">Parameters to create cache key prefix</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
        {
            prefix = PrepareKeyPrefix(prefix, prefixParameters);
            _perRequestCache.RemoveByPrefix(prefix);
            await _distributedCache.RemoveAsync(prefix);
            await _distributedCache.RemoveByPrefixAsync(prefix);
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public Task ClearAsync()
        {
            return _distributedCache.RemoveByPrefixAsync(null);
        }

        /// <summary>
        /// Perform some action with exclusive lock
        /// </summary>
        /// <param name="resource">The key we are locking on</param>
        /// <param name="expirationTime">The time after which the lock will automatically be expired</param>
        /// <param name="action">Action to be performed with locking</param>
        /// <returns>True if lock was acquired and action was performed; otherwise false</returns>
        public bool PerformActionWithLock(string resource, TimeSpan expirationTime, Action action)
        {
            //ensure that lock is acquired
            if (!string.IsNullOrEmpty(_distributedCache.GetString(resource)))
                return false;

            try
            {
                _distributedCache.SetString(resource, resource, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expirationTime
                });

                //perform action
                action();

                return true;
            }
            finally
            {
                //release lock even if action fails
                _distributedCache.Remove(resource);
            }
        }



        (string, string) GetDataTypeInfo(object data, JsonSerializerSettings serializerSettings)
        {
            var typeInfo = GetTypeInfo(data.GetType());
            return (typeInfo, JsonConvert.SerializeObject(data, serializerSettings));
        }

        string GetTypeInfo(Type type)
        {
            var typeNmae = $"{type.Namespace}.{type.Name}";
            if (type.IsGenericType && type.GenericTypeArguments.Length > 0)
            {
                var list = new List<string>();
                var argsTypes = type.GenericTypeArguments.ToArray();
                for (int i = 0; i < argsTypes.Length; i++)
                {
                    list.Add(GetTypeInfo(argsTypes[i]));
                }
                typeNmae += $"[{string.Join(", ", list.Select(p => $"[{p}]"))}]";
            }
            return $"{typeNmae}, {type.Assembly.GetName().Name}";
        }

        (bool, T) GetObject<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return (false, default);
            }
            var (typeInfo, dataJson) = JsonConvert.DeserializeObject<(string, string)>(json);
            if (string.IsNullOrEmpty(typeInfo))
            {
                return (false, default);
            }
            var type = Type.GetType(typeInfo);
            if (type == null || !typeof(T).IsAssignableFrom(type))
            {
                return (false, default);
            }
            if (string.IsNullOrEmpty(dataJson))
                return (false, default);

            var item = (T)JsonConvert.DeserializeObject(dataJson, type);
            return (true, item);
        }

        #endregion

        #region Nested class

        /// <summary>
        /// Represents a manager for caching during an HTTP request (short term caching)
        /// </summary>
        protected class PerRequestCache
        {
            #region Fields

            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly ReaderWriterLockSlim _lockSlim;

            #endregion

            #region Ctor

            public PerRequestCache(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;

                _lockSlim = new ReaderWriterLockSlim();
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Get a key/value collection that can be used to share data within the scope of this request
            /// </summary>
            protected virtual IDictionary<object, object> GetItems()
            {
                return _httpContextAccessor.HttpContext?.Items;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Get a cached item. If it's not in the cache yet, then load and cache it
            /// </summary>
            /// <typeparam name="T">Type of cached item</typeparam>
            /// <param name="key">Cache key</param>
            /// <param name="acquire">Function to load item if it's not in the cache yet</param>
            /// <returns>The cached value associated with the specified key</returns>
            public virtual T Get<T>(string key, Func<T> acquire)
            {
                IDictionary<object, object> items;

                using (new ReaderWriteLockDisposable(_lockSlim, ReaderWriteLockType.Read))
                {
                    items = GetItems();
                    if (items == null)
                        return acquire == null ? default : acquire.Invoke();

                    //item already is in cache, so return it
                    if (items[key] != null)
                        return (T)items[key];
                }

                //or create it using passed function
                var result = acquire == null ? default : acquire.Invoke();

                //and set in cache (if cache time is defined)
                using (new ReaderWriteLockDisposable(_lockSlim))
                    items[key] = result;

                return result;
            }

            /// <summary>
            /// Add the specified key and object to the cache
            /// </summary>
            /// <param name="key">Key of cached item</param>
            /// <param name="data">Value for caching</param>
            public virtual void Set(string key, object data)
            {
                if (data == null)
                    return;

                using (new ReaderWriteLockDisposable(_lockSlim))
                {
                    var items = GetItems();
                    if (items == null)
                        return;

                    items[key] = data;
                }
            }

            /// <summary>
            /// Get a value indicating whether the value associated with the specified key is cached
            /// </summary>
            /// <param name="key">Key of cached item</param>
            /// <returns>True if item already is in cache; otherwise false</returns>
            public virtual bool IsSet(string key)
            {
                using (new ReaderWriteLockDisposable(_lockSlim, ReaderWriteLockType.Read))
                {
                    var items = GetItems();
                    return items?[key] != null;
                }
            }

            /// <summary>
            /// Remove the value with the specified key from the cache
            /// </summary>
            /// <param name="key">Key of cached item</param>
            public virtual void Remove(string key)
            {
                using (new ReaderWriteLockDisposable(_lockSlim))
                {
                    var items = GetItems();
                    items?.Remove(key);
                }
            }

            /// <summary>
            /// Remove items by key prefix
            /// </summary>
            /// <param name="prefix">String key prefix</param>
            public virtual void RemoveByPrefix(string prefix)
            {
                using (new ReaderWriteLockDisposable(_lockSlim, ReaderWriteLockType.UpgradeableRead))
                {
                    var items = GetItems();
                    if (items == null)
                        return;

                    //get cache keys that matches pattern
                    var regex = new Regex(prefix,
                        RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var matchesKeys = items.Keys.Select(p => p.ToString())
                        .Where(key => regex.IsMatch(key ?? string.Empty)).ToList();

                    if (!matchesKeys.Any())
                        return;

                    using (new ReaderWriteLockDisposable(_lockSlim))
                        //remove matching values
                        foreach (var key in matchesKeys)
                            items.Remove(key);
                }
            }

            #endregion
        }

        #endregion
    }
}
