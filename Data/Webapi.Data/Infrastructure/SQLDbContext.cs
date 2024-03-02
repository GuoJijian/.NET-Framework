using Webapi.Core.Configuration;
using Webapi.Core.Domain;
using Webapi.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using Webapi.Core;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Webapi.Data.Infrastructure
{
    /// <summary>
    /// Object context
    /// </summary>
    public class SQLDbContext : DbContext, IDbContext
    {
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options">DataBase Context Options</param>
        public SQLDbContext(DbContextOptions options) : base(options)
        {
        }

        public bool EnsureCreated()
        {
            return Database.EnsureCreated();
        }
        public void TryMigrate()
        {
            var sdfsd = Database?.GetPendingMigrations();
            if (sdfsd != null && sdfsd.Any())
            {
                Database.Migrate();
            }
        }
        #endregion

        #region Utilities

        /// <summary>
        /// On model creating
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            var prefix = Database.GetService<AppSettings>().Get<DataConfig>().DbTablePrefix;
            var typeFinder = Database.GetService<ITypeFinder>();

            var typesToRegister = typeFinder.FindClassesOfType<IModelRelationalMapping>().ToArray();
            foreach (var type in typesToRegister)
            {
                var mapping = Activator.CreateInstance(type) as IModelRelationalMapping;
                mapping.OnMapping(modelBuilder, prefix);
            }
            var options = Database.GetService<DbContextOptions>();
            var applicationServiceProvider = options.Extensions.OfType<CoreOptionsExtension>().Single().ApplicationServiceProvider;
            var visiter = applicationServiceProvider.GetService<IModelVisiter>();
            visiter?.Visit(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        #endregion
    }
}