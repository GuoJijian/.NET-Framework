using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Webapi.Core.Domain
{
    /// <summary>
    /// 用户基类
    /// </summary>
    public abstract class User : BaseEntity, IUser
    {
        public string Name { get; set; }

        public string PasswordHash { get; set; }
        public bool block { get; set; }
    }
}
