using Newtonsoft.Json;

namespace Webapi.Core
{
    public partial interface IConfig
    {
        /// <summary>
        /// Gets a section name to load configuration
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        string Name => GetType().Name;

        /// <summary>
        /// Gets an order of configuration
        /// </summary>
        /// <returns>Order</returns>
        public int GetOrder() => 1;
    }
}
