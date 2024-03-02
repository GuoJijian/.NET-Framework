using Webapi.Core.Domain;
using System;
using System.Threading.Tasks;

namespace Webapi.Core
{
    /// <summary>
    /// Consumer interface
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public interface IConsumer<T> : IDisposable
    {
        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventMessage">Event</param>
        Task HandleEventAsync(T eventMessage);

        int Order { get; }
    }
}