using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Webapi.Core.Logging.LinedLogger;

namespace Webapi.Core.Logging.FileLogger
{
    [ProviderAlias("FileLogger")]
    public class FileLoggerProvider : ILoggerProvider, IDisposable
    {
        Microsoft.Extensions.Logging.ILogger Logger { get; }
        public FileLoggerProvider(LinedLogWriter linedLogWriter, IExternalScopeProvider scopeProvider, IEnumerable<ConsoleFormatter> consoleFormatters)
        {
            Logger = new FileLogger(linedLogWriter, scopeProvider, consoleFormatters.Single(p => p.Name == ConsoleFormatterNames.Simple));
        }
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return Logger;
        }

        public void Dispose()
        {
        }
    }
}
