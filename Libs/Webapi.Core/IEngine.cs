using Autofac;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webapi.Core
{
    public interface IEngine
    {
        IServiceProvider ServiceProvider { get; }


        /// <summary>
        /// Resolve dependency
        /// </summary>
        /// <param name="scope">Scope</param>
        /// <typeparam name="T">Type of resolved service</typeparam>
        /// <returns>Resolved service</returns>
        T Resolve<T>(IServiceScope scope = null) where T : class;

        /// <summary>
        /// Resolve dependency
        /// </summary>
        /// <param name="type">Type of resolved service</param>
        /// <param name="scope">Scope</param>
        /// <returns>Resolved service</returns>
        object Resolve(Type type, IServiceScope scope = null);
        T ResolveWithParam<T>(object key, Parameter[] parameters) where T : class;
        ILifetimeScope CreateLifetimeScope();

        /// <summary>
        /// Resolve dependencies
        /// </summary>
        /// <typeparam name="T">Type of resolved services</typeparam>
        /// <returns>Collection of resolved services</returns>
        IEnumerable<T> ResolveAll<T>(string key = "", ILifetimeScope scope = null);

        /// <summary>
        /// Resolve unregistered service
        /// </summary>
        /// <param name="type">Type of service</param>
        /// <returns>Resolved service</returns>
        object ResolveUnregistered(Type type);

    }
}
