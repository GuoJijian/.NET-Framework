using Webapi.Core;
using Webapi.Core.Domain;
using System;
using System.Net;
using System.Threading;
using Webapi.Core.Domain.Logs;
using System.Threading.Tasks;

namespace Webapi.Framework.Logging {
    /// <summary>
    /// Logging extensions
    /// </summary>
    public static class LoggingExtensions {
        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task DebugAsync<TUser, TLog>(this ILogger<TUser, TLog> logger, string message, TUser user, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null) 
            where TLog : Log, IUserLog<TUser>, new()
            where TUser : User
        {
            return FilteredLog(logger, LogLevel.Debug, message, user, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task InformationAsync<TUser, TLog>(this ILogger<TUser, TLog> logger, string message, TUser user, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null) 
            where TLog : Log, IUserLog<TUser>, new()
            where TUser : User
        {
            return FilteredLog(logger, LogLevel.Information, message, user, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task WarningAsync<TUser, TLog>(this ILogger<TUser, TLog> logger, string message, TUser user, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null) 
            where TLog : Log, IUserLog<TUser>, new()
            where TUser : User
        {
            return FilteredLog(logger, LogLevel.Warning, message, user, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task ErrorAsync<TUser, TLog>(this ILogger<TUser, TLog> logger, string message, TUser user, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null) 
            where TLog : Log, IUserLog<TUser>, new()
            where TUser : User
        {
            return FilteredLog(logger, LogLevel.Error, message, user, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Fatal
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task FatalAsync<TUser, TLog>(this ILogger<TUser, TLog> logger, string message, TUser user,  Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null) 
            where TLog : Log, IUserLog<TUser>, new()
            where TUser : User
        {
            return FilteredLog(logger, LogLevel.Fatal, message, user, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Add a log records
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="level">Level</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        private static async Task FilteredLog<TUser, TLog>(ILogger<TUser, TLog> logger, LogLevel level, string message, TUser user, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null) 
            where TLog : Log, IUserLog<TUser>, new()
            where TUser : User
        {
            //don't log thread abort exception
            if (exception is ThreadAbortException)
                return;

            if (logger.IsEnabled(level)) {
                var fullMessage = exception?.ToString() ?? string.Empty;
                await logger.InsertLog(level, message, fullMessage, user, endPoint, pageUrl, referrerUrl);
            }
        }


        /// <summary>
        /// Debug
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task DebugAsync(this ILogger logger, string message, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null) {
            return FilteredLog(logger, LogLevel.Debug, message, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task InformationAsync(this ILogger logger, string message, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null)
        {
            return FilteredLog(logger, LogLevel.Information, message, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task WarningAsync(this ILogger logger, string message, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null)
        {
            return FilteredLog(logger, LogLevel.Warning, message, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task ErrorAsync(this ILogger logger, string message, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null)
        {
            return FilteredLog(logger, LogLevel.Error, message, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Fatal
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        public static Task FatalAsync(this ILogger logger, string message, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null)
        {
            return FilteredLog(logger, LogLevel.Fatal, message, exception, endPoint, pageUrl, referrerUrl);
        }

        /// <summary>
        /// Add a log records
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="level">Level</param>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="user">Customer</param>
        private static async Task FilteredLog(ILogger logger, LogLevel level, string message, Exception exception = null, EndPoint endPoint = null, string pageUrl = null, string referrerUrl = null)
        {
            //don't log thread abort exception
            if (exception is ThreadAbortException)
                return;

            if (logger.IsEnabled(level))
            {
                var fullMessage = exception?.ToString() ?? string.Empty;
                await logger.InsertLog(level, message, fullMessage, endPoint, pageUrl, referrerUrl);
            }
        }
    }
}
