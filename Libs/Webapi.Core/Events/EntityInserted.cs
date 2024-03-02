using Webapi.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Webapi.Core.Events {
    /// <summary>
    /// A container for entities that have been inserted.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityInserted<T> {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="entity">Entity</param>
        public EntityInserted(T entity) {
            Entity = entity;
        }

        /// <summary>
        /// Entity
        /// </summary>
        public T Entity { get; private set; }
    }
}
