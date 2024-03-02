using Webapi.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webapi.Core.Domain.Logs;
using Webapi.Core.Domain;

namespace Webapi.Data.Mapping
{
    public abstract class LogBaseMap<T> : EntityTypeConfiguration<T> where T : Log {

        public override void Configure(EntityTypeBuilder<T> builder) {
            builder.HasIndex(p => p.CreatedTime);
            builder.HasIndex(p => p.ShortMessage);
            builder.HasIndex(p => new { p.CreatedTime, p.ShortMessage });
        }
    }

    public abstract class UserLogBaseMap<TUser, TLog> : LogBaseMap<TLog>
        where TLog : Log, IUserLog<TUser>
        where TUser : User
    {

        public override void Configure(EntityTypeBuilder<TLog> builder)
        {
            builder.HasIndex(p => p.CreatedTime);
            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => p.ShortMessage);
            builder.HasIndex(p => new { p.CreatedTime, p.ShortMessage });
            builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
        }
    }
}
