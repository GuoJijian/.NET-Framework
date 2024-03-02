using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webapi.Core;
using Webapi.Data.Infrastructure;
using Webapi.Core.Domain;

namespace Webapi.Data.Mapping
{

    public class SettingMap : EntityTypeConfiguration<Setting>
    {

        public override void Configure(EntityTypeBuilder<Setting> builder)
        {
            builder.HasIndex(p => p.ModuleId);
            builder.Property(u => u.Name).IsRequired().IsSmallString();
            builder.Property(u => u.Value).IsRequired().IsLargeString();
        }
    }    
}
