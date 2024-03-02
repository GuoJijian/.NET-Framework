using Webapi.Core;
using Webapi.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Webapi.Data.Infrastructure
{
    public abstract class EntityTypeConfiguration<T> : IEntityTypeConfiguration<T>, IModelRelationalMapping where T : BaseEntity {
        bool Haskey;
        public EntityTypeConfiguration(bool haskey = true) {
            Haskey = haskey;
        }

        public void OnMapping(ModelBuilder modelBuilder, string tablenameprefix)
        {
            var builder = modelBuilder.Entity<T>();
            builder.HasIndex(p => p.Deleted);
            builder.HasQueryFilter(p => !p.Deleted);
            builder.ToTable(tablenameprefix + typeof(T).Name.ToLower());
            if (Haskey)
            {
                builder.HasKey(p => p.Id);
            }
            Configure(builder);
        }

        public virtual void Configure(EntityTypeBuilder<T> builder) {

        }
    }
}
