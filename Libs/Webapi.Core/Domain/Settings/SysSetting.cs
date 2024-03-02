using Webapi.Core.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Webapi.Core.Domain {

    public class SysSetting : Setting {
        public SysSetting() { }
        public SysSetting(uint id, string name, string value, int appid) : base(id, name, value) {

        }
    }
}