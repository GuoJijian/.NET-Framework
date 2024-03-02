using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webapi.Admins.Main
{
    class AuthorizeDescription
    {
        [Category("后台管理")]
        [Description("一般管理员")]
        public const string admin = "admin";

        /// <summary>
        /// 内建超级管理员,不接受管理的权限
        /// </summary>
        //[Category("后台管理")]
        //[Description("内建超级管理员")]
        public const string buildinAdmin = "buildinAdmin";

        [Category("后台管理")]
        [Description("管理用户")]
        public const string userManage = "userManage";

        [Category("后台管理")]
        [Description("查看用户")]
        public const string userList = "userList";

    }
}
