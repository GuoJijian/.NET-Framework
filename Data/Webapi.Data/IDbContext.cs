using Webapi.Core.Domain;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;
using System.Threading.Tasks;

namespace Webapi.Data
{
    /// <summary>
    /// DB context
    /// </summary>
    public interface IDbContext {

        bool EnsureCreated();
        void TryMigrate();
        /// <summary>
        /// Get DbSet
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>DbSet</returns>
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        /// <summary>
        /// Entry
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        /// <summary>
        /// Save changes
        /// </summary>
        /// <returns>Result</returns>
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));


        #region Properties

        public  ChangeTracker ChangeTracker { get; }

        #endregion
    }
}
