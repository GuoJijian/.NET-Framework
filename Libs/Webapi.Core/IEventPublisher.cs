using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Webapi.Core {
    /// <summary>
    /// Evnt publisher
    /// </summary>
    public interface IEventPublisher {
        /// <summary>
        /// Publish event
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="eventMessage">Event message</param>
        Task PublishAsync<T>(T eventMessage);
    }
}
