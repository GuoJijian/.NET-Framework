using Autofac;
using System;
using System.Linq;
using Webapi.Core;
using Webapi.Core.Domain;
using Webapi.Core.Domain.Logs;
using Webapi.Framework.EventPublish;
using Webapi.Framework.Logging;

namespace Webapi.Framework
{
    public class DependencyRegistrar : IDependencyRegistrar<AppSettings> {
        public int Order => 10;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings) {

            builder.RegisterGeneric(typeof(LoggerAdapter<,>)).As(typeof(ILogger<,>)).InstancePerLifetimeScope();

            builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
            builder.RegisterType<SubscriptionService>().As<ISubscriptionService>().SingleInstance();

            builder.RegisterType<Logger>().As<ILogger>().InstancePerLifetimeScope();

            //other event consumers
            var consumers = typeFinder.FindClassesOfType(typeof(IConsumer<>)).ToList();
            foreach (var consumer in consumers) {
                builder.RegisterType(consumer)
                    .As(consumer.FindInterfaces((type, criteria) => {
                        var isMatch = type.IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                        return !consumer.IsGenericType && isMatch;
                    }, typeof(IConsumer<>)))
                    .InstancePerLifetimeScope();
            }
        }
    }
}
