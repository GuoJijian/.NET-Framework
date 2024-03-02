using Autofac;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Autofac.Core;
using Webapi.Core;
using Webapi.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Webapi.Core.Infrastructure
{
    public static class IocEngine
    {
        public static void SetEngine(IEngine engine) => Current = engine;
        public static IEngine Current { get; private set; }

        public static IServiceProvider ServiceProvider => Current.ServiceProvider;
        public static T ResolveUnregistered<T>() => (T)Current.ResolveUnregistered(typeof(T));
        public static object ResolveUnregistered(Type type) => Current.ResolveUnregistered(type);
        public static object Resolve(Type type, IServiceScope scope = null) => Current.Resolve(type, scope);




        #region Methods

        public static T Resolve<T>(object key = null) where T : class => Current.Resolve<T>();
        public static T ResolveWithParam<T>(object key = null, params Parameter[] parameters) where T : class => Current.ResolveWithParam<T>(key, parameters);

        public static ILifetimeScope CreateLifetimeScope() => Current.CreateLifetimeScope();

        /// <summary>
        ///  Resolve dependency
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static object Resolve(Type type) => Current.Resolve(type);

        /// <summary>
        /// Resolve dependencies
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <returns></returns>
        public static T[] ResolveAll<T>() => Current.ResolveAll<T>().ToArray();
        #endregion

        /// <summary>
        /// Run startup tasks
        /// </summary>
        public static void RunStartupTasks(ITypeFinder typeFinder)
        {
            var startUpTaskTypes = typeFinder.FindClassesOfType<IStartupTask>();
            var startUpTasks = new List<IStartupTask>();
            foreach (var startUpTaskType in startUpTaskTypes)
                startUpTasks.Add((IStartupTask)Activator.CreateInstance(startUpTaskType));
            //sort
            startUpTasks = startUpTasks.AsQueryable().OrderBy(st => st.Order).ToList();
            foreach (var startUpTask in startUpTasks)
                startUpTask.Execute();
        }
    }
}

