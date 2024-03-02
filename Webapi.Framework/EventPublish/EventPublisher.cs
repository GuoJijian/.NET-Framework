using Webapi.Core;
using Webapi.Core.Domain;
using Webapi.Core.Infrastructure;
using Webapi.Framework.Logging;
using System;
using System.Linq;
using Webapi.Core.Domain.Logs;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Webapi.Framework.EventPublish
{
    /// <summary>
    /// Evnt publisher
    /// </summary>
    public class EventPublisher : IEventPublisher
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="subscriptionService"></param>
        public EventPublisher(ISubscriptionService subscriptionService, IServiceScopeFactory serviceScopeFactory)
        {
            _subscriptionService = subscriptionService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Publish event
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="eventMessage">Event message</param>
        public virtual async Task PublishAsync<T>(T eventMessage)
        {
            //get all event subscribers, excluding from not installed plugins
            var subscribers = _subscriptionService.GetSubscriptions<T>().OrderBy(p => p.Order)
                .ToList();

            //publish event to subscribers
            foreach (var subscriber in subscribers)
            {
                try
                {
                    using (subscriber)
                    {
                        await subscriber.HandleEventAsync(eventMessage);
                    }
                }
                catch (Exception ex)
                {
                    var logger = _serviceScopeFactory.CreateScope().ServiceProvider.GetService<ILogger>();
                    if (logger == null)
                        return;

                    await logger.ErrorAsync(ex.Message, ex);
                }
            }
        }
    }
}
