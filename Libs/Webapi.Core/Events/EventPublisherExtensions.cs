using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core.Domain;

namespace Webapi.Core.Events
{
    /// <summary>
    /// Event publisher extensions
    /// </summary>
    public static class EventPublisherExtensions
    {
        /// <summary>
        /// Entity inserted
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public static async Task EntityInsertedAsync<TEntity>(this IEventPublisher eventPublisher, TEntity entity) where TEntity : BaseEntity
        {
            await eventPublisher.PublishAsync(new EntityInserted<TEntity>(entity));
        }

        public static async Task EntityInsertedAsync<TEntity>(this IEventPublisher eventPublisher, IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            await eventPublisher.PublishAsync(new EntityInserted<IEnumerable<TEntity>>(entities));
        }

        /// <summary>
        /// Entity updated
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public static async Task EntityUpdatedAsync<TEntity>(this IEventPublisher eventPublisher, TEntity entity) where TEntity : BaseEntity
        {
            await eventPublisher.PublishAsync(new EntityUpdated<TEntity>(entity));
        }

        public static async Task EntityUpdatedAsync<TEntity>(this IEventPublisher eventPublisher, IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            await eventPublisher.PublishAsync(new EntityUpdated<IEnumerable<TEntity>>(entities));
        }

        /// <summary>
        /// Entity deleted
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="eventPublisher">Event publisher</param>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public static async Task EntityDeletedAsync<TEntity>(this IEventPublisher eventPublisher, TEntity entity) where TEntity : BaseEntity
        {
            await eventPublisher.PublishAsync(new EntityDeleted<TEntity>(entity));
        }

        public static async Task EntityDeletedAsync<TEntity>(this IEventPublisher eventPublisher, IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            await eventPublisher.PublishAsync(new EntityDeleted<IEnumerable<TEntity>>(entities));
        }
    }
}
