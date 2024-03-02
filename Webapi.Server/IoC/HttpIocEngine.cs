using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Webapi.Core;
using Webapi.Core.Infrastructure;

namespace Webapi.Server.IoC
{
    public class HttpIocEngine : IEngine
    {
        #region Fields & props
        public IServiceProvider ServiceProvider { get; private set; }
        public IContainer Container { get; private set; }

        #endregion

        #region Utilities


        /// <summary>
        /// Register dependencies
        /// </summary>
        /// <param name="context">Config</param>
        public IContainer RegisterDependencies(ContainerBuilder builder, ITypeFinder typeFinder,IConfiguration configuration, AppSettings appSettings)
        {

            //-----------------------查找所有实现了依赖接口的对象/即加载插件项目-------------------------
            var drTypes = typeFinder.FindClassesOfType<IDependencyRegistrar<AppSettings>>();
            var drInstances = new List<IDependencyRegistrar<AppSettings>>();
            foreach (var drType in drTypes)
            {
                drInstances.Add((IDependencyRegistrar<AppSettings>)Activator.CreateInstance(drType));
            }

            //-----------------------按优先级排序--------------------------------------------------------
            drInstances = drInstances.AsQueryable().OrderBy(t => t.Order).ToList();

            //-----------------------执行依赖注册过程----------------------------------------------------
            foreach (var dependencyRegistrar in drInstances)
            {
                dependencyRegistrar.Register(builder, typeFinder, appSettings);
            }
            Container = builder.Build();
            ServiceProvider = new AutofacServiceProvider(Container);
            return Container;
        }

        #endregion

        #region Methods

        IServiceProvider GetServiceProvider(IServiceScope scope = null)
        {
            if (scope == null)
            {
                var accessor = Container.Resolve<IHttpContextAccessor>();
                var context = accessor?.HttpContext;
                return context?.RequestServices ?? ServiceProvider;
            }
            return scope.ServiceProvider;
        }
        public object Resolve(Type type, IServiceScope scope = null)
        {
            return GetServiceProvider(scope)?.GetService(type);
        }
        /// <summary>
        /// Resolve dependency
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <returns></returns>
        public T Resolve<T>(object key = null, ILifetimeScope scope = null) where T : class
        {
            if (scope == null)
            {
                //no scope specified
                scope = CreateLifetimeScope();
            }
            if (key == null)
            {
                return scope.Resolve<T>();
            }
            return scope.ResolveKeyed<T>(key);
        }
        public T ResolveWithParam<T>(object key = null, params Parameter[] parameters) where T : class
        {
            var scope = CreateLifetimeScope();
            if (key == null)
            {
                return scope.Resolve<T>(parameters);
            }
            return scope.ResolveKeyed<T>(key, parameters);
        }

        public ILifetimeScope CreateLifetimeScope()
        {
            return Container.BeginLifetimeScope();
        }

        /// <summary>
        ///  Resolve dependency
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public object Resolve(Type type)
        {
            return Container.Resolve(type);
        }

        /// <summary>
        /// Resolve dependencies
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <returns></returns>
        public IEnumerable<T> ResolveAll<T>(string key = "", ILifetimeScope scope = null)
        {
            if (scope == null)
            {
                scope = CreateLifetimeScope();
            }
            if (string.IsNullOrEmpty(key))
            {
                return scope.Resolve<IEnumerable<T>>().ToArray();
            }
            return scope.ResolveKeyed<IEnumerable<T>>(key).ToArray();
        }

        public T Resolve<T>(IServiceScope scope = null) where T : class
        {
            return (T)Resolve(typeof(T), scope);
        }
        public object ResolveUnregistered(Type type)
        {
            Exception innerException = null;
            foreach (var constructor in type.GetConstructors())
            {
                try
                {
                    using (var scope = CreateLifetimeScope())
                    {
                        //try to resolve constructor parameters
                        var parameters = constructor.GetParameters().Select(parameter =>
                        {
                            var service = scope.Resolve(parameter.ParameterType);
                            if (service == null)
                                throw new Exception("Unknown dependency");
                            return service;
                        });

                        //all is ok, so create instance
                        return Activator.CreateInstance(type, parameters.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    innerException = ex;
                }
            }

            throw new Exception("No constructor was found that had all the dependencies satisfied.", innerException);
        }
        #endregion
    }
}
