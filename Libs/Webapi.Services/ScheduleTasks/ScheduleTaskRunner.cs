using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Caching;
using Webapi.Core.Domain.ScheduleTasks;
using Webapi.Core.Infrastructure;
using Webapi.Core.ScheduleTasks;
using Webapi.Framework.Logging;

namespace Webapi.Services.ScheduleTasks
{
    /// <summary>
    /// Schedule task runner
    /// </summary>
    public partial class ScheduleTaskRunner : IScheduleTaskRunner
    {
        #region Fields

        protected readonly ILocker _locker;
        protected readonly ILogger _logger;
        protected readonly IScheduleTaskService _scheduleTaskService;

        public CommonConfig CommonConfig { get; }
        public IServiceScopeFactory ServiceScopeFactory { get; }

        #endregion

        #region Ctor

        public ScheduleTaskRunner(
            ILocker locker,
            ILogger logger,
            IScheduleTaskService scheduleTaskService,
            CommonConfig commonConfig,
            IServiceScopeFactory serviceScopeFactory
            )
        {
            _locker = locker;
            _logger = logger;
            _scheduleTaskService = scheduleTaskService;
            CommonConfig = commonConfig;
            ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Initialize and execute task
        /// </summary>
        protected void ExecuteTask(ScheduleTask scheduleTask)
        {
            var type = Type.GetType(scheduleTask.Type) ??
                       //ensure that it works fine when only the type name is specified (do not require fully qualified names)
                       AppDomain.CurrentDomain.GetAssemblies()
                           .Select(a => a.GetType(scheduleTask.Type))
                           .FirstOrDefault(t => t != null);
            if (type == null)
                throw new Exception($"Schedule task ({scheduleTask.Type}) cannot by instantiated");

            var instance = ServiceScopeFactory.CreateScope().ServiceProvider.GetService(type);
            instance ??= IocEngine.ResolveUnregistered(type);

            if (instance is not IScheduleTask task)
                return;

            task.ExecuteAsync().Wait();
            scheduleTask.LastEndTime = scheduleTask.LastSuccessTime = DateTime.Now;
            scheduleTask.SuccCount++;
            //update appropriate datetime properties
            _scheduleTaskService.UpdateTaskAsync(scheduleTask).Wait();
        }

        /// <summary>
        /// Is task already running?
        /// </summary>
        /// <param name="scheduleTask">Schedule task</param>
        /// <returns>Result</returns>
        protected virtual bool IsTaskAlreadyRunning(ScheduleTask scheduleTask)
        {
            //task run for the first time
            if (!scheduleTask.LastStartTime.HasValue && !scheduleTask.LastEndTime.HasValue)
                return false;

            var lastStart = scheduleTask.LastStartTime ?? DateTime.Now;

            //task already finished
            if (scheduleTask.LastEndTime.HasValue && lastStart < scheduleTask.LastEndTime)
                return false;

            //task wasn't finished last time
            if (lastStart.AddSeconds(scheduleTask.Seconds) <= DateTime.Now)
                return false;

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the task
        /// </summary>
        /// <param name="scheduleTask">Schedule task</param>
        /// <param name="forceRun">Force run</param>
        /// <param name="throwException">A value indicating whether exception should be thrown if some error happens</param>
        /// <param name="ensureRunOncePerPeriod">A value indicating whether we should ensure this task is run once per run period</param>
        public async Task ExecuteAsync(ScheduleTask scheduleTask, bool forceRun = false, bool throwException = false, bool ensureRunOncePerPeriod = true)
        {
            var enabled = forceRun || (scheduleTask?.Enabled ?? false);

            if (scheduleTask == null || !enabled)
                return;

            if (ensureRunOncePerPeriod)
            {
                //task already running
                if (IsTaskAlreadyRunning(scheduleTask))
                    return;

                //validation (so nobody else can invoke this method when he wants)
                if (scheduleTask.LastStartTime.HasValue && (DateTime.Now - scheduleTask.LastStartTime).Value.TotalSeconds < scheduleTask.Seconds)
                    //too early
                    return;
            }

            scheduleTask.LastStartTime = DateTime.Now;
            //update appropriate datetime properties
            await _scheduleTaskService.UpdateTaskAsync(scheduleTask);
            try
            {
                //get expiration time
                var expirationInSeconds = Math.Min(scheduleTask.Seconds, 300) - 1;
                var expiration = TimeSpan.FromSeconds(expirationInSeconds);

                //execute task with lock
                _locker.PerformActionWithLock(scheduleTask.Type, expiration, () => ExecuteTask(scheduleTask));
            }
            catch (Exception exc)
            {
                var scheduleTaskUrl = $"{CommonConfig.DefaultSiteUrl}{NopTaskDefaults.ScheduleTaskPath}";

                scheduleTask.Enabled = !scheduleTask.StopOnError;
                scheduleTask.LastEndTime = DateTime.Now;
                scheduleTask.FailCount++;
                await _scheduleTaskService.UpdateTaskAsync(scheduleTask);

                var message = string.Format("The \"{0}\" scheduled task failed with the \"{1}\" error (Task type: \"{2}\". Task run address: \"{3}\").", scheduleTask.Name,
                    exc.Message, scheduleTask.Type, scheduleTaskUrl);

                //log error
                await _logger.ErrorAsync(message, exc);
                if (throwException)
                    throw;
            }
        }

        #endregion
    }
}
