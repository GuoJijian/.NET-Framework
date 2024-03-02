using Autofac;
using Webapi.Core;
using Webapi.Services.ScheduleTasks;

namespace Webapi.Services {
    public class DependencyRegistrar : IDependencyRegistrar<AppSettings> {
        public int Order => 10;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings) {

            //schedule tasks
            builder.RegisterType<TaskScheduler>().As<ITaskScheduler>().SingleInstance();
            builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleTaskRunner>().As<IScheduleTaskRunner>().InstancePerLifetimeScope();

        }
    }
}
