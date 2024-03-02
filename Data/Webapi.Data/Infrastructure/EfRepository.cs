using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Caching;
using Webapi.Core.Domain;
using Webapi.Core.Events;

namespace Webapi.Data.Infrastructure
{

    /// <summary>
    /// Entity Framework repository
    /// </summary>
    public partial class EfRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity, new()
    {
        #region Fields
        private DbSet<TEntity> _entities;

        private readonly IDbContext _context;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStaticCacheManager _staticCacheManager;
        #endregion

        #region Properties

        public IEventPublisher EventPublisher => _eventPublisher;

        public IDbContext DbContext => _context;

        /// <summary>
        /// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        public virtual IQueryable<TEntity> TableNoTracking => Entities.AsNoTracking();


        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IQueryable<TEntity> Table => Entities;

        /// <summary>
        /// Entities
        /// </summary>
        protected virtual DbSet<TEntity> Entities => _entities ??= DbContext.Set<TEntity>();

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="context">Object context</param>
        public EfRepository(IEventPublisher eventPublisher, IDbContext context, IStaticCacheManager staticCacheManager)
        {
            this._context = context;
            this._eventPublisher = eventPublisher;
            this._staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Method

        public async Task<TEntity> GetEntityAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false, params Expression<Func<TEntity, object>>[] properties)
        {
            var array = await GetEntitiesAsync(predicate, includeDeleted, properties);
            return array.FirstOrDefault();
        }

        public async Task<IList<TEntity>> GetEntitiesAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = CheckDeletedTag(Table, includeDeleted);
            foreach (var property in includeProperties)
            {
                query = query.Include(property);
            }
            return await query.Where(predicate).ToArrayAsync();
        }

        /// <summary>
        /// Adds "deleted" filter to query which contains <see cref="ISoftDeletedEntity"/> entries, if its need
        /// </summary>
        /// <param name="query">Entity entries</param>
        /// <param name="includeDeleted">Whether to include deleted items</param>
        /// <returns>Entity entries</returns>
        protected virtual IQueryable<TEntity> CheckDeletedTag(IQueryable<TEntity> query, bool includeDeleted)
        {
            return includeDeleted ? query.IgnoreQueryFilters() : query;
        }


        /// <summary>
        /// Get the entity entry
        /// </summary>
        /// <param name="id">Entity entry identifier</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity entry
        /// </returns>
        public virtual async Task<TEntity> GetByIdAsync(uint? id, Func<IStaticCacheManager, CacheKey> getCacheKey = null, bool includeDeleted = false, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            if (!id.HasValue || id == 0)
                return null;

            async Task<TEntity> getEntityAsync()
            {
                var query = CheckDeletedTag(Table, includeDeleted);
                foreach (var property in includeProperties)
                {
                    query = query.Include(property);
                }
                return (await query.FirstOrDefaultAsync(entity => entity.Id == id)) ?? new TEntity();
            }

            //caching
            var cacheKey = getCacheKey?.Invoke(_staticCacheManager)
                ?? _staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<TEntity>.ByIdCacheKey, id);

            var entity = await _staticCacheManager.GetAsync(cacheKey, getEntityAsync);
            return entity.Id != 0 ? entity : null;
        }

        /// <summary>
        /// Get entity entries by identifiers
        /// </summary>
        /// <param name="ids">Entity entry identifiers</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="Nop.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity entries
        /// </returns>
        public virtual async Task<IList<TEntity>> GetByIdsAsync(IList<uint> ids, Func<IStaticCacheManager, CacheKey> getCacheKey = null, bool includeDeleted = false, params Expression<Func<TEntity, object>>[] includeProperties)
        {
            if (ids == null || !ids.Any())
                return new List<TEntity>();

            async Task<IList<TEntity>> getByIdsAsync()
            {
                var query = CheckDeletedTag(Table, includeDeleted);
                foreach (var property in includeProperties)
                {
                    query = query.Include(property);
                }

                //get entries
                var entries = await query.Where(entry => ids.Contains(entry.Id)).OrderBy(p => p.Id).ToListAsync();
                return entries;
            }

            if (getCacheKey == null)
                return await getByIdsAsync();

            //caching
            var cacheKey = getCacheKey(_staticCacheManager)
                ?? _staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<TEntity>.ByIdsCacheKey, ids);
            return await _staticCacheManager.GetAsync(cacheKey, getByIdsAsync);
        }

        /// <summary>
        /// Get all entity entries
        /// </summary>
        /// <param name="func">Function to select entries</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="Nop.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity entries
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null,
            bool includeDeleted = false)
        {
            var query = CheckDeletedTag(Table, includeDeleted);
            query = func != null ? func(query) : query;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get all entity entries
        /// </summary>
        /// <param name="func">Function to select entries</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="Nop.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity entries
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAllAsync(
            Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> func = null, bool includeDeleted = false)
        {
            var query = CheckDeletedTag(Table, includeDeleted);
            query = func != null ? await func(query) : query;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get paged list of all entity entries
        /// </summary>
        /// <param name="func">Function to select entries</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">Whether to get only the total number of entries without actually loading data</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="Nop.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the paged list of entity entries
        /// </returns>
        public virtual async Task<IPagedList<TEntity>> GetAllPagedAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = false)
        {
            var query = CheckDeletedTag(Table, includeDeleted);

            query = func != null ? func(query) : query;
            return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        /// <summary>
        /// Insert the entity entry
        /// </summary>
        /// <param name="entity">Entity entry</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<int> InsertAsync(TEntity entity, bool publishEvent = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await Entities.AddAsync(entity);
            var count = await DbContext.SaveChangesAsync();
            //event notification
            if (publishEvent)
            {
                await _eventPublisher.EntityInsertedAsync(entity);
            }

            return count;
        }

        /// <summary>
        /// Insert entity entries
        /// </summary>
        /// <param name="entities">Entity entries</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<int> InsertAsync(IList<TEntity> entities, bool publishEvent = true)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await Entities.AddRangeAsync(entities);
            var count = await DbContext.SaveChangesAsync();

            if (publishEvent)
            {
                await _eventPublisher.EntityInsertedAsync(entities);
            }
            return count;
        }

        /// <summary>
        /// Loads the original copy of the entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the copy of the passed entity
        /// </returns>
        public virtual async Task<TEntity> LoadOriginalCopyAsync(TEntity entity)
        {
            return await Table.FirstOrDefaultAsync(e => e.Id == entity.Id);
        }

        /// <summary>
        /// Update the entity entry
        /// </summary>
        /// <param name="entity">Entity entry</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<int> UpdateAsync(TEntity entity, bool publishEvent = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Entities.Update(entity);
            var count = await DbContext.SaveChangesAsync();

            //event notification
            if (publishEvent)
            {
                await _eventPublisher.EntityUpdatedAsync(entity);
            }

            return count;
        }

        /// <summary>
        /// Update entity entries
        /// </summary>
        /// <param name="entities">Entity entries</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<int> UpdateAsync(IList<TEntity> entities, bool publishEvent = true)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var count = entities.Count;
            if (count > 0)
            {
                Entities.UpdateRange(entities);
                count = await DbContext.SaveChangesAsync();

                //event notification
                if (publishEvent)
                {
                    await _eventPublisher.EntityUpdatedAsync(entities);
                }
            }
            return count;
        }

        /// <summary>
        /// Delete the entity entry
        /// </summary>
        /// <param name="entity">Entity entry</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<int> DeleteAsync(TEntity entity, bool publishEvent = true)
        {
            entity.Deleted = true;
            Entities.Update(entity);
            var count = await DbContext.SaveChangesAsync();

            //event notification
            if (publishEvent)
            {
                await _eventPublisher.EntityDeletedAsync(entity);
            }

            return count;
        }

        /// <summary>
        /// Delete entity entries
        /// </summary>
        /// <param name="entities">Entity entries</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<int> DeleteAsync(IList<TEntity> entities, bool publishEvent = true)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                entity.Deleted = true;
            }
            Entities.UpdateRange(entities);
            var count = await DbContext.SaveChangesAsync();

            //event notification
            if (publishEvent)
            {
                await _eventPublisher.EntityDeletedAsync(entities);
            }

            return count;
        }

        /// <summary>
        /// Delete entity entries by the passed predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of deleted records
        /// </returns>
        public virtual async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, bool publishEvent = true)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var entities = await Table.Where(predicate).ToArrayAsync();

            return await DeleteAsync(entities, publishEvent);
        }

        /// <summary>
        /// Truncates database table
        /// </summary>
        /// <param name="resetIdentity">Performs reset identity column</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<int> TruncateAsync()
        {
            return await DeleteAsync(p => !p.Deleted);
        }

        #endregion
    }
}