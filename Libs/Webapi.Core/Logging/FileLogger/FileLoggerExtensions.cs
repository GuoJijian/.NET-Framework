using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;
using System;
using Webapi.Core.Logging.LinedLogger;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using Webapi.Core.Utils;
using Microsoft.AspNetCore.HttpLogging;

namespace Webapi.Core.Logging.FileLogger
{
    public static class FileLoggerExtensions
    {

        public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddSingleton(NullExternalScopeProvider.Instance);
            builder.Services.TryAddSingleton(p =>
            {
                var options = p.GetService<IOptionsMonitor<LinedLogWriterOptions>>();
                var environment = p.GetService<IHostEnvironment>();
                var consoleOption = p.GetService<IOptionsMonitor<ConsoleLoggerOptions>>();

                var formatters = p.GetService<IEnumerable<ConsoleFormatter>>().ToList();

                var opts = formatters.Select(p => p.Private<ConsoleFormatterOptions>("FormatterOptions")).Where(p => p != null);

                opts.ForEach(opt =>
                {
                    opt.TimestampFormat = "HH:MM:ss fffffff";
                });

                var consoleLoggerProvider = new ConsoleLoggerProvider(consoleOption, formatters);
                return new LinedLogger.LinedLogWriter(options, environment, consoleLoggerProvider);
            });
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<LinedLogWriterOptions, FileLoggerProvider>(builder.Services);

            return builder;
        }
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<LinedLogWriterOptions> configure)
        {
            builder.AddFile();
            builder.Services.Configure(configure);
            return builder;
        }


        public static ILoggingBuilder AddW3CLogging(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.AddW3CLogging(logging =>
            {
                logging.LoggingFields = W3CLoggingFields.All;
                logging.FileSizeLimit = 32 * 1024 * 1024;
                logging.RetainedFileCountLimit = 20;
                logging.FileName = "w3cLog_";
                logging.LogDirectory = $"{NopConfigurationDefaults.AppSettingsDirectory}\\logs";
                logging.FlushInterval = TimeSpan.FromSeconds(2);
            });
            LoggerProviderOptions.RegisterProviderOptions<W3CLoggerOptions, W3CLoggerOptionsProvider>(builder.Services);
            return builder;
        }
    }

    [ProviderAlias("w3c")]
    class W3CLoggerOptionsProvider
    {
    }
}
