using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Webapi.Core.Domain;
using Webapi.Core.Domain.Logs;

namespace Webapi.Core
{

    /// <summary>
    /// Logger interface
    /// </summary>
    public partial interface ILogger<TUser, TLog>
        where TLog : Log, IUserLog<TUser>, new()
        where TUser : User
    {
        /// <summary>
        /// Determines whether a log level is enabled
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Result</returns>
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// Deletes a log item
        /// </summary>
        /// <param name="log">Log item</param>
        Task<int> DeleteLog(TLog log);

        /// <summary>
        /// Deletes a log items
        /// </summary>
        /// <param name="logs">Log items</param>
        Task<int> DeleteLogs(IList<TLog> logs);

        /// <summary>
        /// Clears a log
        /// </summary>
        Task ClearLog();

        /// <summary>
        /// Gets all log items
        /// </summary>
        /// <param name="fromTime">Log item creation from; null to load all records</param>
        /// <param name="toTime">Log item creation to; null to load all records</param>
        /// <param name="message">Message</param>
        /// <param name="logLevel">Log level; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Log item items</returns>
        IPagedList<TLog> GetAllLogs(DateTime? fromTime = null, DateTime? toTime = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a log item
        /// </summary>
        /// <param name="logId">Log item identifier</param>
        /// <returns>Log item</returns>
        Task<TLog> GetLogById(uint logId);

        /// <summary>
        /// Get log items by identifiers
        /// </summary>
        /// <param name="logIds">Log item identifiers</param>
        /// <returns>Log items</returns>
        IList<TLog> GetLogByIds(uint[] logIds);

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="shortMessage">The short message</param>
        /// <param name="fullMessage">The full message</param>
        /// <param name="customer">The customer to associate log record with</param>
        /// <returns>A log item</returns>
        Task<TLog> InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", TUser user = null, EndPoint endPoing = null, string pageUrl = null, string referrerUrl = null);
    }
}