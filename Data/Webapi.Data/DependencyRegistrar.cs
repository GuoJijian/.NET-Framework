using Autofac;
using Webapi.Core;
using Webapi.Core.Configuration;
using Webapi.Data.Infrastructure;
using Webapi.Data.Services;

namespace Webapi.Data
{
    public class DependencyRegistrar : IDependencyRegistrar<AppSettings>
    {
        public int Order => 9;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings)
        {
            var distributedCacheConfig = appSettings.Get<DistributedCacheConfig>();


            builder.RegisterType<SQLDbContext>().As<IDbContext>().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();

            builder.RegisterType<SettingService>().As<ISettingService>().InstancePerLifetimeScope();

            builder.RegisterSource(new SettingsSource());
        }
    }
}
