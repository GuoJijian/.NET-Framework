using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System;
using System.Text;
using Microsoft.Extensions.Logging.Console;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Webapi.Core.Logging.LinedLogger
{
    public class LinedLogWriter
    {
        private readonly FileLoggerProcessor _messageQueue;
        private readonly IOptionsMonitor<LinedLogWriterOptions> _options;

        public LinedLogWriter(IOptionsMonitor<LinedLogWriterOptions> options, IHostEnvironment environment, /*ILoggerFactory loggerFactory*/ILoggerProvider loggerProvider)
        {
            _options = options;
            _options.OnChange(options =>
            {
            });
            _messageQueue = InitializeMessageQueue(_options, environment, loggerProvider/*s.OfType<ConsoleLoggerProvider>().FirstOrDefault()*/);
        }

        // Virtual for testing
        internal virtual FileLoggerProcessor InitializeMessageQueue(IOptionsMonitor<LinedLogWriterOptions> options, IHostEnvironment environment, ILoggerProvider consoleLogger)
        {
            return new FileLoggerProcessor(options, environment, consoleLogger);
        }

        public void Log(string elements)
        {
            _messageQueue.EnqueueMessage(elements);
        }

        public Microsoft.Extensions.Logging.LogLevel MinLevel => _options.CurrentValue.MinLevel;
    }
}
