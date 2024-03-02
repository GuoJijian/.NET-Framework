using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Configuration;
using Webapi.Core.Domain.ScheduleTasks;
using Webapi.Core.ScheduleTasks;

namespace Webapi.Core.Logging
{

    public partial class ClearLogTask : IScheduleTask
    {
        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public ClearLogTask(CommonSettings commonSettings,
            ILogger logger)
        {
            _commonSettings = commonSettings;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual async Task ExecuteAsync()
        {
            var now = DateTime.Now;

            await _logger.ClearLogAsync(_commonSettings.ClearLogOlderThanDays == 0 ? null : now.AddDays(-_commonSettings.ClearLogOlderThanDays));
        }

        #endregion
    }
}
