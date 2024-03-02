using Webapi.Core;
using Webapi.Core.Domain;
using Webapi.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Webapi.Data.Mapping
{
    public abstract class UserMap<T> : EntityTypeConfiguration<T> where T : User
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            base.Configure(builder);
            //builder.HasIndex(u => u.Name).IsUnique();
            builder.Property(u => u.Name).IsRequired().IsSmallString().IsUnicode();
            builder.Property(u => u.PasswordHash).IsRequired().IsUnicode(false).IsSmallString();
            builder.HasIndex(u => u.block);
        }
    }
}