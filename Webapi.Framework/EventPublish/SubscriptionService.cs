using Webapi.Core;
using Webapi.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Webapi.Framework.EventPublish {
    /// <summary>
    /// Event subscription service
    /// </summary>
    public class SubscriptionService : ISubscriptionService {

        public SubscriptionService(IServiceScopeFactory serviceScopeFactory)
        {
            ServiceScopeFactory = serviceScopeFactory;
        }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        /// <summary>
        /// Get subscriptions
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Event consumers</returns>
        public IList<IConsumer<T>> GetSubscriptions<T>() {
            return ServiceScopeFactory.CreateScope().ServiceProvider.GetServices<IConsumer<T>>().ToList();
        }
    }
}
