using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using Nop.Core.Configuration;
//using Nop.Web.Framework.Infrastructure.Extensions;
using System;
using Webapi.Core;

namespace Webapi.Server
{

    class DefaultServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        public DefaultServiceProviderFactory(ITypeFinder typeFinder, IConfiguration configuration, AppSettings appSettings, Func<ContainerBuilder, ITypeFinder, IConfiguration, AppSettings, IContainer> configurationAction)
        {
            TypeFinder = typeFinder;
            HostBuilderContext = configuration;
            AppSettings = appSettings;
            ConfigurationAction = configurationAction;
        }

        Func<ContainerBuilder, ITypeFinder, IConfiguration, AppSettings, IContainer> ConfigurationAction { get; }
        public ITypeFinder TypeFinder { get; }
        IConfiguration HostBuilderContext { get; }
        public AppSettings AppSettings { get; }

        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);
            return containerBuilder;
        }

        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            var container = ConfigurationAction(containerBuilder, TypeFinder, HostBuilderContext, AppSettings);
            return new AutofacServiceProvider(container);
        }
    }
}
