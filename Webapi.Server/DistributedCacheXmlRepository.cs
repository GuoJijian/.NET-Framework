using System;
using Webapi.Core;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Webapi.Core.Caching;

namespace Webapi.Server
{
    class DistributedCacheXmlRepository : IXmlRepository
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        private readonly static int CacheTime = (int)TimeSpan.FromDays(20 * 365).TotalMinutes;
        private readonly IRedisDistributedCache _redisDistributedCache;

        public DistributedCacheXmlRepository(IStaticCacheManager staticCacheManager, IRedisDistributedCache redisDistributedCache, ILoggerFactory loggerFactory)
        {
            this.StaticCacheManager = staticCacheManager;
            this._redisDistributedCache = redisDistributedCache;
            _logger = loggerFactory.CreateLogger<DistributedCacheXmlRepository>();
        }

        const string prefix = "DataProtection.PersistKeys.";

        public IStaticCacheManager StaticCacheManager { get; }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            _logger.Log(LogLevel.Information, $"GetAllElements begin");
            return GetAllElementsCore().ToList().AsReadOnly();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            _logger.Log(LogLevel.Information, $"StoreElement begin: {friendlyName}");
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var fullKey = prefix + friendlyName;
            using (var tempFileStream = new MemoryStream())
            {
                element.Save(tempFileStream);
                var array = tempFileStream.ToArray();
                var base64Str = Convert.ToBase64String(array);
                StaticCacheManager.SetAsync(new Core.Caching.CacheKey(fullKey) { CacheTime = CacheTime }, base64Str).Wait();
            }
            _logger.Log(LogLevel.Information, $"StoreElement end: {friendlyName}");
        }

        private IEnumerable<XElement> GetAllElementsCore()
        {
            var keys = _redisDistributedCache.Keys(prefix).ToEnumerable().ToArray();
            foreach (var key in keys)
            {
                var fullKey = key;
                var base64Str = StaticCacheManager.Get<string>(new CacheKey(fullKey) { CacheTime = CacheTime }, () => null);
                if (!string.IsNullOrEmpty(base64Str))
                {
                    var array = Convert.FromBase64String(base64Str);
                    using (var stream = new MemoryStream(array))
                    {
                        _logger.Log(LogLevel.Information, $"GetAllElementsCore begin：{fullKey}");
                        yield return XElement.Load(stream);
                        _logger.Log(LogLevel.Information, $"GetAllElementsCore end：{fullKey}");
                    }
                }
            }
        }
    }
}
