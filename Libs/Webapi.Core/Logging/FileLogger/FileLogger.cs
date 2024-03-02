using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading;
using System;
using Webapi.Core.Logging.LinedLogger;
using Microsoft.Extensions.Logging.Abstractions;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging.Console;
using System.Xml.Linq;

namespace Webapi.Core.Logging.FileLogger
{
    public class FileLogger : Microsoft.Extensions.Logging.ILogger
    {
        public LinedLogWriter LinedLogWriter { get; }
        public IExternalScopeProvider ScopeProvider { get; }
        public ConsoleFormatter ConsoleFormatter { get; }

        public FileLogger(LinedLogWriter linedLogWriter, IExternalScopeProvider scopeProvider, ConsoleFormatter consoleFormatter)
        {
            LinedLogWriter = linedLogWriter;
            ScopeProvider = scopeProvider;
            ConsoleFormatter = consoleFormatter;
        }
        class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
        Disposable _DisposableInstance = new Disposable();
        public IDisposable BeginScope<TState>(TState state)
        {
            return _DisposableInstance;
        }
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return LinedLogWriter.MinLevel <= logLevel;
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!this.IsEnabled(logLevel))
                return;

            var log = string.Concat($"{DateTime.Now.ToString("HH:mm:ss fffffff")}\t[{logLevel}]\t[{Thread.CurrentThread.ManagedThreadId}], \t{formatter.Invoke(state, exception)}");
            if (exception != null)
            {
                log += Environment.NewLine + exception;
            }
            this.LinedLogWriter.Log(log);//.TrimEnd(Environment.NewLine.ToCharArray())
        }
    }
}
