using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Webapi.Core;
using System.Configuration;

namespace Webapi.Data.Infrastructure
{
    public partial class DataConfig : IConfig
    {

        /// <summary>
        /// Gets or sets a connection string
        /// </summary>
        public string DbTablePrefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a data provider
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DataProviderType DataProvider { get; set; } = DataProviderType.SQLite;

        /// <summary>
        /// Gets or sets the wait time (in seconds) before terminating the attempt to execute a command and generating an error.
        /// By default, timeout isn't set and a default value for the current provider used. 
        /// Set 0 to use infinite timeout.
        /// </summary>
        public int? SQLCommandTimeout { get; set; } = null;

        /// <summary>
        /// Gets a section name to load configuration
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string Name => nameof(ConfigurationManager.ConnectionStrings);

        /// <summary>
        /// Gets an order of configuration
        /// </summary>
        /// <returns>Order</returns>
        public int GetOrder() => 0; //display first

        /// <summary>
        /// Gets or sets a connection string
        /// </summary>
        public string ConnectionStringSQLite { get; set; } = "Filename=testdb.db";

        /// <summary>
        /// Gets or sets a connection string
        /// </summary>
        public string ConnectionStringMySQL { get; set; } = "server=192.168.1.70;userid=root;pwd=mysql8;port=3307;database=testdb;charset='utf8m4b';sslmode=none;";

        public string ConnectionStringPostgreSQL { get; set; } = "server=192.168.0.35;userid=root;pwd=654321;port=3307;database=game;charset='utf8';sslmode=none;";


        /// <summary>
        /// Gets or sets a connection string
        /// </summary>
        public string ConnectionStringMSSQL { get; set; } = "data source=192.168.1.70;initial catalog=dbs_his;user id=sa;password=123456";

        [System.Text.Json.Serialization.JsonIgnore]
        public string ConnectionString
        {
            get
            {
                switch (DataProvider)
                {
                    case DataProviderType.SqlServer:
                        return ConnectionStringMSSQL;
                    case DataProviderType.MySql:
                        return ConnectionStringMySQL;
                    case DataProviderType.PostgreSQL:
                        return ConnectionStringPostgreSQL;
                    case DataProviderType.SQLite:
                        return ConnectionStringSQLite;
                    case DataProviderType.Unknown:
                    default:
                        throw new System.NotSupportedException();
                }
            }
        }
    }

}
