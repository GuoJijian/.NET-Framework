using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using Webapi.Core;
using Webapi.Data.Infrastructure;

namespace Webapi.Data.MySQL
{
    public class DependencyRegistrar : IDependencyRegistrar<AppSettings>
    {
        public int Order => 8;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings)
        {
            var dataConfig = appSettings.Get<DataConfig>();
            if (dataConfig.DataProvider == DataProviderType.MySql)
            {
                if (string.IsNullOrEmpty(dataConfig.ConnectionString))
                    throw new ArgumentOutOfRangeException(nameof(dataConfig.ConnectionString));

                builder.Register((IComponentContext context) =>
                {
                    var dbOptionsbuilder = new DbContextOptionsBuilder().UseMySql(dataConfig.ConnectionString, ServerVersion.AutoDetect(dataConfig.ConnectionString), mysqlDBContextOptionBuilder =>
                    {
                        mysqlDBContextOptionBuilder.MigrationsAssembly(appSettings.GetType().Assembly.GetName().Name).MigrationsHistoryTable(HistoryRepository.DefaultTableName);
                        var timeout = DataSettingsManager.GetSqlCommandTimeout();
                        if (timeout > 0)
                        {
                            mysqlDBContextOptionBuilder.CommandTimeout(timeout);
                            mysqlDBContextOptionBuilder.EnableRetryOnFailure();
                        }
                    }).UseApplicationServiceProvider(context.Resolve<IServiceProvider>());
                    dbOptionsbuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.MultipleCollectionIncludeWarning));
                    return dbOptionsbuilder.Options;
                }).As<DbContextOptions>().SingleInstance();
                builder.RegisterType<MySQLModelVisiter>().As<IModelVisiter>().InstancePerLifetimeScope();
            }
        }
    }
}
