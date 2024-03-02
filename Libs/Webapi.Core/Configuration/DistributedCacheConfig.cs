
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Webapi.Core.Configuration
{
    public partial class DistributedCacheConfig : IConfig
    {
        /// <summary>
        /// Gets or sets a distributed cache type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DistributedCacheType DistributedCacheType { get; private set; } = DistributedCacheType.Memory;

        /// <summary>
        /// Gets or sets a value indicating whether we should use distributed cache
        /// </summary>
        public bool Enabled { get; private set; } = true;

        /// <summary>
        /// Gets or sets connection string. Used when distributed cache is enabled
        /// </summary>
        public string ConnectionString { get; private set; } = "192.168.1.70:6379,ssl=False";

        /// <summary>
        /// Gets or sets schema name. Used when distributed cache is enabled and DistributedCacheType property is set as SqlServer
        /// </summary>
        public string SchemaName { get; private set; } = "dbo";

        /// <summary>
        /// Gets or sets table name. Used when distributed cache is enabled and DistributedCacheType property is set as SqlServer
        /// </summary>
        public string TableName { get; private set; } = "DistributedCache";
    }
}
