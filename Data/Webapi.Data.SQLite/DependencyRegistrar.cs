using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using Webapi.Core;
using Webapi.Data.Infrastructure;

namespace Webapi.Data.SQLite
{
    public class DependencyRegistrar : IDependencyRegistrar<AppSettings>
    {
        public int Order => 9;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings)
        {
            var dataConfig = appSettings.Get<DataConfig>();
            if (dataConfig.DataProvider == DataProviderType.SQLite)
            {
                if (string.IsNullOrEmpty(dataConfig.ConnectionString))
                    throw new ArgumentOutOfRangeException(nameof(dataConfig.ConnectionString));

                builder.Register((IComponentContext context) =>
                {
                    var dbOptionsbuilder = new DbContextOptionsBuilder().UseSqlite(dataConfig.ConnectionString, dbContextOptionBuilder =>
                    {
                        dbContextOptionBuilder.MigrationsAssembly(appSettings.GetType().Assembly.GetName().Name).MigrationsHistoryTable(HistoryRepository.DefaultTableName);
                    }).UseApplicationServiceProvider(context.Resolve<IServiceProvider>());
                    dbOptionsbuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning));
                    return dbOptionsbuilder.Options;
                }).As<DbContextOptions>().SingleInstance();
            }
        }
    }
}


