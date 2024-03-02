using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core.Caching;
using Webapi.Core.Common;
using Webapi.Core.Domain.ScheduleTasks;
using Webapi.Data.Infrastructure;
using Webapi.Core.Logging;

namespace Webapi.Data.Mapping
{
    public class ScheduleTaskMap : EntityTypeConfiguration<ScheduleTask>
    {

        public override void Configure(EntityTypeBuilder<ScheduleTask> builder)
        {
            builder.HasIndex(p => p.Name);
            var lastEnabledTime = DateTime.Now;
            var keepAliveTaskType = typeof(KeepAliveTask);
            var clearCacheTaskType = typeof(ClearCacheTask);
            var clearLogTaskType = typeof(ClearLogTask);
            builder.HasData(
                new ScheduleTask()
                {
                    Id = 1,
                    Name = "Keep alive",
                    Seconds = 300,
                    Type = $"{keepAliveTaskType.FullName}, {keepAliveTaskType.Assembly.GetName().Name}",
                    Enabled = true,
                    LastEnabledTime = lastEnabledTime,
                    StopOnError = false
                },
                new ScheduleTask
                {
                    Id = 2,
                    Name = "Clear cache",
                    Seconds = 3600,
                    Type = $"{clearCacheTaskType.FullName}, {clearCacheTaskType.Assembly.GetName().Name}",
                    Enabled = false,
                    LastEnabledTime = lastEnabledTime,
                    StopOnError = false
                },
                new ScheduleTask
                {
                    Id = 3,
                    Name = "Clear log",
                    //60 minutes
                    Seconds = 3600,
                    Type = $"{clearLogTaskType.FullName}, {clearLogTaskType.Assembly.GetName().Name}",
                    Enabled = false,
                    LastEnabledTime = lastEnabledTime,
                    StopOnError = false
                }
            );
        }
    }
}
