using System;
using System.IO;

namespace Webapi.Core
{
    public static partial class NopConfigurationDefaults
    {
        /// <summary>
        /// Gets the directory that contains app settings
        /// </summary>
        public static DirectoryInfo AppSettingsDirectory { get; } = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "App_Data"));

        /// <summary>
        /// Gets the path to file that contains app settings
        /// </summary>
        public static string AppSettingsFilePath => Path.Combine(AppSettingsDirectory.FullName, "appsettings.json");
    }
}
