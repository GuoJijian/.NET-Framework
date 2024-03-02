using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webapi.Services.ScheduleTasks
{
    public interface ITaskScheduler
    {
        /// <summary>
        /// Initializes task scheduler
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Starts the task scheduler
        /// </summary>
        public void StartScheduler();

        /// <summary>
        /// Stops the task scheduler
        /// </summary>
        public void StopScheduler();
    }
}
