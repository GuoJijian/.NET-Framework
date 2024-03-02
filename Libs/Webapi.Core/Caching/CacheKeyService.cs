using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core.Configuration;
using Webapi.Core.Domain;

namespace Webapi.Core.Caching
{
    public abstract partial class CacheKeyService
    {
        #region Constants

        /// <summary>
        /// Gets an algorithm used to create the hash value of identifiers need to cache
        /// </summary>
        private string HashAlgorithm => "SHA1";

        #endregion

        #region Fields

        protected readonly AppSettings _appSettings;

        #endregion

        #region Ctor

        protected CacheKeyService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare the cache key prefix
        /// </summary>
        /// <param name="prefix">Cache key prefix</param>
        /// <param name="prefixParameters">Parameters to create cache key prefix</param>
        protected virtual string PrepareKeyPrefix(string prefix, params object[] prefixParameters)
        {
            return prefixParameters?.Any() ?? false
                ? string.Format(prefix, prefixParameters.Select(CreateCacheKeyParameters).ToArray())
                : prefix;
        }

        /// <summary>
        /// Create the hash value of the passed identifiers
        /// </summary>
        /// <param name="ids">Collection of identifiers</param>
        /// <returns>String hash value</returns>
        protected virtual string CreateIdsHash(IEnumerable<uint> ids)
        {
            var identifiers = ids.ToList();

            if (!identifiers.Any())
                return string.Empty;

            var identifiersString = string.Join(", ", identifiers.OrderBy(id => id));
            return HashHelper.CreateHash(Encoding.UTF8.GetBytes(identifiersString), HashAlgorithm);
        }

        protected virtual string CreateIdsHash(IEnumerable<string> filters)
        {
            var strlist = filters.ToList();

            if (!strlist.Any())
                return string.Empty;

            var identifiersString = string.Join(", ", strlist.OrderBy(str => str, StringComparer.OrdinalIgnoreCase));
            return HashHelper.CreateHash(Encoding.UTF8.GetBytes(identifiersString), HashAlgorithm);
        }

        /// <summary>
        /// Converts an object to cache parameter
        /// </summary>
        /// <param name="parameter">Object to convert</param>
        /// <returns>Cache parameter</returns>
        protected virtual object CreateCacheKeyParameters(object parameter)
        {
            return parameter switch
            {
                null => "null",
                IEnumerable<uint> ids => CreateIdsHash(ids),
                IEnumerable<string> filters => CreateIdsHash(filters),
                IEnumerable<BaseEntity> entities => CreateIdsHash(entities.Select(entity => entity.Id)),
                BaseEntity entity => entity.Id,
                decimal param => param.ToString(CultureInfo.InvariantCulture),
                IEnumerable<object> filters => CreateIdsHash(filters.Select(p => p.ToString())),
                _ => parameter
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a copy of cache key and fills it by passed parameters
        /// </summary>
        /// <param name="cacheKey">Initial cache key</param>
        /// <param name="cacheKeyParameters">Parameters to create cache key</param>
        /// <returns>Cache key</returns>
        public virtual CacheKey PrepareKey(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            return cacheKey.Create(CreateCacheKeyParameters, cacheKeyParameters);
        }

        /// <summary>
        /// Create a cache key and fills it by passed parameters
        /// </summary>
        /// <param name="cacheKey">Initial cache key</param>
        /// <param name="cacheKeyParameters">Parameters to create cache key</param>
        /// <returns>Cache key</returns>
        public virtual CacheKey PrepareKey<T>(string keyTemplate, params object[] cacheKeyParameters) where T : BaseEntity
        {
            var key = string.Format(keyTemplate, cacheKeyParameters.Select(CreateCacheKeyParameters).ToArray());
            var cacheKey = new CacheKey(key, NopEntityCacheDefaults<T>.Prefix);
            return cacheKey;
        }

        /// <summary>
        /// Create a copy of cache key using the default cache time and fills it by passed parameters
        /// </summary>
        /// <param name="cacheKey">Initial cache key</param>
        /// <param name="cacheKeyParameters">Parameters to create cache key</param>
        /// <returns>Cache key</returns>
        public virtual CacheKey PrepareKeyForDefaultCache(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            var key = cacheKey.Create(CreateCacheKeyParameters, cacheKeyParameters);

            key.CacheTime = _appSettings.Get<CacheConfig>().DefaultCacheTime;

            return key;
        }

        /// <summary>
        /// Create a cache key and fills it by passed parameters
        /// </summary>
        /// <param name="keyTemplate"></param>
        /// <param name="cacheKeyParameters"></param>
        /// <returns></returns>
        public virtual CacheKey PrepareKeyForDefaultCache<T>(string keyTemplate, params object[] cacheKeyParameters) where T : BaseEntity
        {
            var key = string.Format(keyTemplate, cacheKeyParameters.Select(CreateCacheKeyParameters).ToArray());
            var cacheKey = new CacheKey(key, NopEntityCacheDefaults<T>.Prefix);
            cacheKey.CacheTime = _appSettings.Get<CacheConfig>().DefaultCacheTime;

            return cacheKey;
        }

        /// <summary>
        /// Create a copy of cache key using the short cache time and fills it by passed parameters
        /// </summary>
        /// <param name="cacheKey">Initial cache key</param>
        /// <param name="cacheKeyParameters">Parameters to create cache key</param>
        /// <returns>Cache key</returns>
        public virtual CacheKey PrepareKeyForShortTermCache(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            var key = cacheKey.Create(CreateCacheKeyParameters, cacheKeyParameters);

            key.CacheTime = _appSettings.Get<CacheConfig>().ShortTermCacheTime;

            return key;
        }


        /// <summary>
        /// Create a cache key using the short cache time and fills it by passed parameters
        /// </summary>
        /// <param name="keyTemplate"></param>
        /// <param name="cacheKeyParameters"></param>
        /// <returns></returns>
        public virtual CacheKey PrepareKeyForShortTermCache<T>(string keyTemplate, params object[] cacheKeyParameters) where T : BaseEntity
        {
            var key = string.Format(keyTemplate, cacheKeyParameters.Select(CreateCacheKeyParameters).ToArray());
            var cacheKey = new CacheKey(key, NopEntityCacheDefaults<T>.Prefix);
            cacheKey.CacheTime = _appSettings.Get<CacheConfig>().ShortTermCacheTime;

            return cacheKey;
        }

        #endregion
    }
}
