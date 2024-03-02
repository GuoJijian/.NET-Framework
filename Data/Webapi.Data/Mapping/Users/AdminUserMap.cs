using Webapi.Core.Domain;
using Webapi.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Webapi.Core;

namespace Webapi.Data.Mapping
{
    public class AdminUserMap : UserMap<Admin>
    {

        public override void Configure(EntityTypeBuilder<Admin> builder)
        {
            base.Configure(builder);

            //set index
            builder.HasIndex(u => u.Telephone);
            builder.HasIndex(u => u.IdNumber);


            //set property constraints
            builder.Property(u => u.Telephone).IsSmallString().IsUnicode(false);
            builder.Property(u => u.IdNumber).IsSmallString().IsUnicode(false);
            builder.Property(u => u.idcFrontUrl).IsMediumString();
            builder.Property(u => u.idcBackUrl).IsMediumString();

            builder.Property(u => u.SText1).IsSmallString();
            builder.Property(u => u.SText2).IsSmallString();
            builder.Property(u => u.SText3).IsSmallString();
            builder.Property(u => u.MText1).IsMediumString();
            builder.Property(u => u.MText2).IsMediumString();
            builder.Property(u => u.MText3).IsMediumString();
            builder.Property(u => u.LText1).IsLargeString();
            builder.Property(u => u.LText2).IsLargeString();
            builder.Property(u => u.LText3).IsLargeString();


            builder.HasData(
                new Admin()
                {
                    Id = 1,
                    Name = "admin",
                    PasswordHash = PasswordUtil.GetPasswordHash("987654"),
                    BuildIn = true,
                    Datetime = DateTime.Now,
                    Roles = null,
                    Remark = "内置超级管理员"
                }, 
                new Admin()
                {
                    Id = 2,
                    Name = "admin1",
                    PasswordHash = PasswordUtil.GetPasswordHash("987654"),
                    BuildIn = false,
                    Datetime = DateTime.Now,
                    Roles = "userManage,userList",
                    Remark = "一般管理员"
                }, 
                new Admin()
                {
                    Id = 3,
                    Name = "admin2",
                    PasswordHash = PasswordUtil.GetPasswordHash("987654"),
                    BuildIn = false,
                    Datetime = DateTime.Now,
                    Roles = "userList",
                    Remark = "一般管理员"
                },
                new Admin()
                {
                    Id = 4,
                    Name = "admin3",
                    PasswordHash = PasswordUtil.GetPasswordHash("987654"),
                    BuildIn = false,
                    block = true,
                    Datetime = DateTime.Now,
                    Roles = "userManage,userList",
                    Remark = "一股管理员(封禁)"
                }
            );
        }
    }
}