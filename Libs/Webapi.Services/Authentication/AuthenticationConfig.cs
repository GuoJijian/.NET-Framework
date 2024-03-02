using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core;

namespace Webapi.Services.Authentication
{
    public class AuthenticationConfig : List<AuthenticationScheme>, IConfig
    {
        public AuthenticationConfig()
        {

        }
    }
    public class AuthenticationScheme
    {
        public string Name { get; set; } = "default";
        public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromDays(30);
        public bool SlidingExpiration { get; set; } = true;
        /// <summary>
        /// 严格模式，严格模式下只允许单端登录
        /// </summary>
        public bool Strictly { get; set; } = true;
    }

}
