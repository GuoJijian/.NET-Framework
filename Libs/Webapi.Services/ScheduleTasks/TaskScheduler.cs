using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Webapi.Core;
using Webapi.Core.Configuration;
using Webapi.Core.Domain.ScheduleTasks;
using Webapi.Core.Http;
using Webapi.Core.Infrastructure;
using Webapi.Data;
using Webapi.Framework.Logging;
using Webapi.Services.ScheduleTasks;

namespace Webapi.Services.ScheduleTasks
{
    /// <summary>
    /// Represents task manager
    /// </summary>
    public partial class TaskScheduler : ITaskScheduler
    {
        #region Fields

        protected static readonly List<TaskThread> _taskThreads = new();
        protected readonly AppSettings _appSettings;
        protected readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public TaskScheduler(AppSettings appSettings,
            IHttpClientFactory httpClientFactory,
            IScheduleTaskService scheduleTaskService,
            IServiceScopeFactory serviceScopeFactory)
        {
            _appSettings = appSettings;
            TaskThread.HttpClientFactory = httpClientFactory;
            _scheduleTaskService = scheduleTaskService;
            TaskThread.ServiceScopeFactory = serviceScopeFactory;
            
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the task manager
        /// </summary>
        public async Task InitializeAsync()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            if (_taskThreads.Any())
                return;

            //initialize and start schedule tasks
            var scheduleTasks = (await _scheduleTaskService.GetAllTasksAsync())
                .OrderBy(x => x.Seconds)
                .ToList();

            //var store = await _storeContext.GetCurrentStoreAsync();
            var commanConfig = _appSettings.Get<CommonConfig>();
            var scheduleTaskUrl = $"{commanConfig.DefaultSiteUrl}{NopTaskDefaults.ScheduleTaskPath}";
            var timeout = commanConfig.ScheduleTaskRunTimeout;

            foreach (var scheduleTask in scheduleTasks)
            {
                var taskThread = new TaskThread(scheduleTask, scheduleTaskUrl, timeout)
                {
                    Seconds = scheduleTask.Seconds
                };

                //sometimes a task period could be set to several hours (or even days)
                //in this case a probability that it'll be run is quite small (an application could be restarted)
                //calculate time before start an interrupted task
                if (scheduleTask.LastStartTime.HasValue)
                {
                    //seconds left since the last start
                    var secondsLeft = (DateTime.Now - scheduleTask.LastStartTime).Value.TotalSeconds;

                    if (secondsLeft >= scheduleTask.Seconds)
                        //run now (immediately)
                        taskThread.InitSeconds = 0;
                    else
                        //calculate start time
                        //and round it (so "ensureRunOncePerPeriod" parameter was fine)
                        taskThread.InitSeconds = (int)(scheduleTask.Seconds - secondsLeft) + 1;
                }
                else if (scheduleTask.LastEnabledTime.HasValue)
                {
                    //seconds left since the last enable
                    var secondsLeft = (DateTime.Now - scheduleTask.LastEnabledTime).Value.TotalSeconds;

                    if (secondsLeft >= scheduleTask.Seconds)
                        //run now (immediately)
                        taskThread.InitSeconds = 0;
                    else
                        //calculate start time
                        //and round it (so "ensureRunOncePerPeriod" parameter was fine)
                        taskThread.InitSeconds = (int)(scheduleTask.Seconds - secondsLeft) + 1;
                }
                else
                    //first start of a task
                    taskThread.InitSeconds = scheduleTask.Seconds;

                _taskThreads.Add(taskThread);
            }
        }

        /// <summary>
        /// Starts the task scheduler
        /// </summary>
        public void StartScheduler()
        {
            foreach (var taskThread in _taskThreads)
                taskThread.InitTimer();
        }

        /// <summary>
        /// Stops the task scheduler
        /// </summary>
        public void StopScheduler()
        {
            foreach (var taskThread in _taskThreads)
                taskThread.Dispose();
        }

        #endregion

        #region Nested class

        /// <summary>
        /// Represents task thread
        /// </summary>
        protected class TaskThread : IDisposable
        {
            #region Fields

            protected readonly string _scheduleTaskUrl;
            protected readonly ScheduleTask _scheduleTask;
            protected readonly int? _timeout;

            protected Timer _timer;
            protected bool _disposed;

            internal static IHttpClientFactory HttpClientFactory { get; set; }
            internal static IServiceScopeFactory ServiceScopeFactory { get; set; }

            #endregion

            #region Ctor

            public TaskThread(ScheduleTask task, string scheduleTaskUrl, int? timeout)
            {
                _scheduleTaskUrl = scheduleTaskUrl;
                _scheduleTask = task;
                _timeout = timeout;

                Seconds = 10 * 60;
            }

            #endregion

            #region Utilities

            private async Task RunAsync()
            {
                if (Seconds <= 0)
                    return;

                StartedTime = DateTime.Now;
                IsRunning = true;
                HttpClient client = null;

                try
                {
                    //create and configure client
                    client = HttpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
                    if (_timeout.HasValue)
                        client.Timeout = TimeSpan.FromMilliseconds(_timeout.Value);

                    //send post data
                    var data = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("taskType", _scheduleTask.Type) });
                    var resp = await client.PostAsync(_scheduleTaskUrl, data);
                    //var content = await resp.Content.ReadAsStringAsync();
                    //Console.WriteLine(content);
                }
                catch (Exception ex)
                {
                    using var scope = ServiceScopeFactory.CreateScope();
                    var logger = scope.ServiceProvider.GetService<ILogger>();
                    var message = ex.InnerException?.GetType() == typeof(TaskCanceledException) ? "ScheduleTasks.TimeoutError" : ex.Message;
                    message = $"ScheduleTasks.Error, taskName: {_scheduleTask.Name}, ex: {message}, type: {_scheduleTask.Type}, taskUrl: {_scheduleTaskUrl}";
                    await logger.ErrorAsync(message, ex);
                }
                finally
                {
                    client?.Dispose();
                }

                IsRunning = false;
            }

            private void TimerHandler(object state)
            {
                try
                {
                    _timer.Change(-1, -1);

                    RunAsync().Wait();
                }
                catch
                {
                    // ignore
                }
                finally
                {
                    if (!_disposed)
                    {
                        if (RunOnlyOnce)
                            Dispose();
                        else
                            _timer.Change(Interval, Interval);
                    }
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Disposes the instance
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            // Protected implementation of Dispose pattern.
            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                    lock (this)
                        _timer?.Dispose();

                _disposed = true;
            }

            /// <summary>
            /// Inits a timer
            /// </summary>
            public void InitTimer()
            {
                _timer ??= new Timer(TimerHandler, null, InitInterval, Interval);
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets or sets the interval in seconds at which to run the tasks
            /// </summary>
            public int Seconds { get; set; }

            /// <summary>
            /// Get or set the interval before timer first start 
            /// </summary>
            public int InitSeconds { get; set; }

            /// <summary>
            /// Get or sets a datetime when thread has been started
            /// </summary>
            public DateTime StartedTime { get; private set; }

            /// <summary>
            /// Get or sets a value indicating whether thread is running
            /// </summary>
            public bool IsRunning { get; private set; }

            /// <summary>
            /// Gets the interval (in milliseconds) at which to run the task
            /// </summary>
            public int Interval
            {
                get
                {
                    //if somebody entered more than "2147483" seconds, then an exception could be thrown (exceeds int.MaxValue)
                    var interval = Seconds * 1000;
                    if (interval <= 0)
                        interval = int.MaxValue;
                    return interval;
                }
            }

            /// <summary>
            /// Gets the due time interval (in milliseconds) at which to begin start the task
            /// </summary>
            public int InitInterval
            {
                get
                {
                    //if somebody entered less than "0" seconds, then an exception could be thrown
                    var interval = InitSeconds * 1000;
                    if (interval <= 0)
                        interval = 0;
                    return interval;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the thread would be run only once (on application start)
            /// </summary>
            public bool RunOnlyOnce { get; set; }

            /// <summary>
            /// Gets a value indicating whether the timer is started
            /// </summary>
            public bool IsStarted => _timer != null;

            /// <summary>
            /// Gets a value indicating whether the timer is disposed
            /// </summary>
            public bool IsDisposed => _disposed;

            #endregion
        }

        #endregion
    }
}
