using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core.Domain.ScheduleTasks;
using Webapi.Core.ScheduleTasks;

namespace Webapi.Core.Common
{
    /// <summary>
    /// Represents a task for keeping the site alive
    /// </summary>
    public partial class KeepAliveTask : IScheduleTask
    {
        #region Fields

        private readonly KeepAliveHttpClient _keepAliveHttpClient;

        #endregion

        #region Ctor

        public KeepAliveTask(KeepAliveHttpClient keepAliveHttpClient)
        {
            _keepAliveHttpClient = keepAliveHttpClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task ExecuteAsync()
        {
            await _keepAliveHttpClient.KeepAliveAsync();
        }

        #endregion
    }
}
