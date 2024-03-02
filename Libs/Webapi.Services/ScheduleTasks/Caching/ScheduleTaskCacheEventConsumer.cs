using Webapi.Core;
using Webapi.Core.Domain.ScheduleTasks;
using Webapi.Services.Caching;

namespace Webapi.Services.ScheduleTasks.Caching
{
    /// <summary>
    /// Represents a schedule task cache event consumer
    /// </summary>
    public partial class ScheduleTaskCacheEventConsumer : CacheEventConsumer<ScheduleTask>
    {
        public ScheduleTaskCacheEventConsumer(IStaticCacheManager staticCacheManager)
            : base(staticCacheManager)
        {

        }
    }
}
