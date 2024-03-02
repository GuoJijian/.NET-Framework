using Autofac;

namespace Webapi.Core {
    /// <summary>
    /// Dependency registrar interface
    /// </summary>
    public interface IDependencyRegistrar<in TContext>
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="context">Initialization context</param>
        void Register(ContainerBuilder builder, ITypeFinder typeFinder, TContext context);

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        int Order { get; }
    }
}
