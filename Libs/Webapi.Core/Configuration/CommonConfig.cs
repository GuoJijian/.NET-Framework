using Newtonsoft.Json;
using System.Collections.Generic;
using Webapi.Core;

namespace Webapi.Core
{
    public partial class CommonConfig : IConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether to display the full error in production environment. It's ignored (always enabled) in development environment
        /// </summary>
        public bool DisplayFullErrorStack { get; private set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to store TempData in the session state. By default the cookie-based TempData provider is used to store TempData in cookies.
        /// </summary>
        public bool UseSessionStateTempDataProvider { get; private set; } = false;

        /// <summary>
        /// The length of time, in milliseconds, before the running schedule task times out. Set null to use default value
        /// </summary>
        public int? ScheduleTaskRunTimeout { get; private set; } = null;

        /// <summary>
        /// Gets or sets a value of "Cache-Control" header value for static content (in seconds)
        /// </summary>
        public string StaticFilesCacheControl { get; private set; } = "public,max-age=31536000";

        /// <summary>
        /// Get or set the blacklist of static file extension for plugin directories
        /// </summary>
        public string PluginStaticFileExtensionsBlacklist { get; private set; } = "";

        /// <summary>
        /// Get or set a value indicating whether to serve files that don't have a recognized content-type
        /// </summary>
        /// <value></value>
        public bool ServeUnknownFileTypes { get; private set; } = false;

        public bool UseStaticFiles { get; set; } = true;
        public string StaticFilesRequestPath { get; set; } = "/static";
        public string StaticFilesRoot { get; set; } = "./static";
        public bool EnableDirectoryBrowsing { get; set; } = true;
        public Dictionary<string, string> ContentTypeMappings { get; set; } = new Dictionary<string, string>();
        public List<string> Urls { get; set; } = new List<string>();

        [System.Text.Json.Serialization.JsonIgnore]
        public string DefaultSiteUrl { get; set; }

        public bool AddResponseCompressionService { get; set; } = true;

        public bool UseHttpDebugLogging { get; set; } = true;
    }
}
