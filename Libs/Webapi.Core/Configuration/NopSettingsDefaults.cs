using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core.Caching;
using Webapi.Core.Domain;

namespace Webapi.Core.Configuration
{
    /// <summary>
    /// Represents default values related to settings
    /// </summary>
    public static partial class NopSettingsDefaults
    {
        #region Caching defaults

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey SettingsAllAsDictionaryCacheKey { get; } = new(NopEntityCacheDefaults<Setting>.AllPrefix + "dictionary.", NopEntityCacheDefaults<Setting>.Prefix);

        #endregion
    }
}
