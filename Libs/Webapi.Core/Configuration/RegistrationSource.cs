using Autofac;
using Autofac.Builder;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Linq;

namespace Webapi.Core.Configuration {

    /// <summary>
    /// Setting source
    /// </summary>
    public abstract class RegistrationSource<T> : RegistrationSource {

        public RegistrationSource():base(typeof(T)) {

        }
    }

    /// <summary>
    /// Setting source
    /// </summary>
    public abstract class RegistrationSource : IRegistrationSource {

        Type serviceType;

        public RegistrationSource(Type type) {
            this.serviceType = type;
        }
        
        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service, 
            Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            var ts = service as TypedService;
            if (ts != null && TypeMatch(ts.ServiceType))
            {
                yield return BuildRegistration(ts.ServiceType);
            }
        }

        protected virtual bool TypeMatch(Type type) {
            return serviceType.IsAssignableFrom(type);
        }

        IComponentRegistration BuildRegistration(Type type) {
            return RegistrationBuilder
                .ForDelegate(type, (context, parameters) => { return Resolve(context, parameters, type); })
                .InstancePerLifetimeScope()
                .CreateRegistration();
        }

        protected abstract object Resolve(IComponentContext context, IEnumerable<Parameter> parameters, Type type);

        /// <summary>
        /// Is adapter for individual components
        /// </summary>
        public bool IsAdapterForIndividualComponents => false;
    }
}
