using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core.Caching;
using Webapi.Core.Domain.ScheduleTasks;
using Webapi.Core.ScheduleTasks;

namespace Webapi.Core.Caching
{
    public partial class ClearCacheTask : IScheduleTask
    {
        #region Fields

        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public ClearCacheTask(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task ExecuteAsync()
        {
            await _staticCacheManager.ClearAsync();
        }

        #endregion
    }
}
