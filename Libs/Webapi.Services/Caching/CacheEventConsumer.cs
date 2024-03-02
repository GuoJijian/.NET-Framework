using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Caching;
using Webapi.Core.Domain;
using Webapi.Core.Events;
using Webapi.Core.Infrastructure;
using Webapi.Data;

namespace Webapi.Services.Caching
{
    public abstract partial class CacheEventConsumer<TEntity> :
        IConsumer<EntityInserted<TEntity>>,
        IConsumer<EntityInserted<IEnumerable<TEntity>>>,
        IConsumer<EntityUpdated<TEntity>>,
        IConsumer<EntityUpdated<IEnumerable<TEntity>>>,
        IConsumer<EntityDeleted<TEntity>>,
        IConsumer<EntityDeleted<IEnumerable<TEntity>>>
        where TEntity : BaseEntity
    {
        #region Fields
        public int Order => 100;
        protected readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        protected CacheEventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Clear cache by entity event type
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task ClearCacheAsync(TEntity entity, EntityEventType entityEventType)
        {
            await RemoveByPrefixAsync(NopEntityCacheDefaults<TEntity>.ByIdsPrefix);
            await RemoveByPrefixAsync(NopEntityCacheDefaults<TEntity>.AllPrefix);
            await RemoveByPrefixAsync(NopEntityCacheDefaults<TEntity>.ByPagedPrefix);

            //if (entityEventType != EntityEventType.Insert)
            await RemoveAsync(NopEntityCacheDefaults<TEntity>.ByIdCacheKey, entity);

            await ClearCacheAsync(entity);
        }

        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual Task ClearCacheAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes items by cache key prefix
        /// </summary>
        /// <param name="prefix">Cache key prefix</param>
        /// <param name="prefixParameters">Parameters to create cache key prefix</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
        {
            await _staticCacheManager.RemoveByPrefixAsync(prefix, prefixParameters);
        }

        /// <summary>
        /// Remove the value with the specified key from the cache
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="cacheKeyParameters">Parameters to create cache key</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task RemoveAsync(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            await _staticCacheManager.RemoveAsync(cacheKey, cacheKeyParameters);
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(EntityInserted<IEnumerable<TEntity>> eventMessage)
        {
            foreach (var entity in eventMessage.Entity)
            {
                await ClearCacheAsync(entity, EntityEventType.Insert);
            }
        }
        /// <summary>
        /// Handle entity inserted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task HandleEventAsync(EntityInserted<TEntity> eventMessage)
        {
            await ClearCacheAsync(eventMessage.Entity, EntityEventType.Insert);
        }

        public async Task HandleEventAsync(EntityUpdated<IEnumerable<TEntity>> eventMessage)
        {
            foreach (var entity in eventMessage.Entity)
            {
                await ClearCacheAsync(entity, EntityEventType.Update);
            }
        }

        /// <summary>
        /// Handle entity updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task HandleEventAsync(EntityUpdated<TEntity> eventMessage)
        {
            await ClearCacheAsync(eventMessage.Entity, EntityEventType.Update);
        }

        public async Task HandleEventAsync(EntityDeleted<IEnumerable<TEntity>> eventMessage)
        {
            foreach (var entity in eventMessage.Entity)
            {
                await ClearCacheAsync(entity, EntityEventType.Delete);
            }
        }

        /// <summary>
        /// Handle entity deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task HandleEventAsync(EntityDeleted<TEntity> eventMessage)
        {
            await ClearCacheAsync(eventMessage.Entity, EntityEventType.Delete);
        }


        public void Dispose()
        {

        }

        #endregion

        #region Nested

        protected enum EntityEventType
        {
            Insert,
            Update,
            Delete
        }

        #endregion
    }
}
