using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using Webapi.Core;
using Webapi.Core.Configuration;
using Webapi.Data.Services;

namespace Webapi.Data
{
    class SettingsSource : RegistrationSource<ISettings>, IRegistrationSource
    {
        protected override object Resolve(IComponentContext context, IEnumerable<Parameter> parameters, Type type)
        {
            var settingService = context.Resolve<ISettingService>();
            return settingService.LoadSettingAsync(type).Result;
        }
    }
}
