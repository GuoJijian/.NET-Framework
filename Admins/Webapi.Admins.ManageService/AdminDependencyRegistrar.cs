using Autofac;
using Webapi.Admins.Main.Service;
using Webapi.Core;

namespace Webapi.Admins.Main
{
    public class AdminDependencyRegistrar : IDependencyRegistrar<AppSettings>
    {
        public int Order => 2000;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings)
        {
            builder.RegisterType<AdminService>().As<IAdminService>().InstancePerLifetimeScope();

        }
    }
}
