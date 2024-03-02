using Webapi.Core;
using Webapi.Core.Configuration;
using Webapi.Core.Domain;
using Webapi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Webapi.Core.Domain.Logs;
using System.Threading.Tasks;

namespace Webapi.Framework.Logging
{

    /// <summary>
    /// LoggerAdapter logger
    /// </summary>
    public class LoggerAdapter<TUser, TLog> : ILogger<TUser, TLog>
        where TLog : Log, IUserLog<TUser>, new()
        where TUser : User
    {
        #region Fields

        private readonly IRepository<TLog> _logRepository;
        CommonSettings _commonSettings;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logRepository">Log repository</param>
        /// <param name="commonSettings">Common settings</param>
        public LoggerAdapter(IRepository<TLog> logRepository, CommonSettings commonSettings)
        {
            this._logRepository = logRepository;
            this._commonSettings = commonSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets a value indicating whether this message should not be logged
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        protected virtual bool IgnoreLog(string message)
        {
            if (!_commonSettings.IgnoreLogWordlist.Any())
                return false;

            if (string.IsNullOrWhiteSpace(message))
                return false;

            return _commonSettings
                .IgnoreLogWordlist
                .Any(x => message.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether a log level is enabled
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Result</returns>
        public virtual bool IsEnabled(LogLevel level)
        {
            //switch (level) {
            //    case LogLevel.Debug:
            //        return false;
            //    default:
            //        return true;
            //}
            return (this._commonSettings.LogLevel > LogLevel.Debug && level >= this._commonSettings.LogLevel) || level > LogLevel.Debug;
        }

        /// <summary>
        /// Deletes a log item
        /// </summary>
        /// <param name="log">Log item</param>
        public virtual Task<int> DeleteLog(TLog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            return _logRepository.DeleteAsync(log);
        }

        /// <summary>
        /// Deletes a log items
        /// </summary>
        /// <param name="logs">Log items</param>
        public virtual Task<int> DeleteLogs(IList<TLog> logs)
        {
            if (logs == null)
                throw new ArgumentNullException(nameof(logs));

            return _logRepository.DeleteAsync(logs);
        }

        /// <summary>
        /// Clears a log
        /// </summary>
        public virtual async Task ClearLog()
        {
            await _logRepository.TruncateAsync();
        }

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
        public virtual IPagedList<TLog> GetAllLogs(DateTime? fromTime = null, DateTime? toTime = null, string message = "", LogLevel? logLevel = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _logRepository.Table;
            if (fromTime.HasValue)
                query = query.Where(l => fromTime.Value <= l.CreatedTime);
            if (toTime.HasValue)
                query = query.Where(l => toTime.Value >= l.CreatedTime);
            if (logLevel.HasValue)
            {
                var logLevelId = logLevel.Value;
                query = query.Where(l => logLevelId == l.LogLevel);
            }
            if (!string.IsNullOrEmpty(message))
                query = query.Where(l => l.ShortMessage.Contains(message) || l.FullMessage.Contains(message));
            query = query.OrderByDescending(l => l.CreatedTime);

            var plog = new PagedList<TLog>(query, pageIndex, pageSize);
            return plog;
        }

        /// <summary>
        /// Gets a log item
        /// </summary>
        /// <param name="logId">Log item identifier</param>
        /// <returns>Log item</returns>
        public virtual Task<TLog> GetLogById(uint logId)
        {
            if (logId == 0)
                return null;

            return _logRepository.GetByIdAsync(logId);
        }

        /// <summary>
        /// Get log items by identifiers
        /// </summary>
        /// <param name="logIds">Log item identifiers</param>
        /// <returns>Log items</returns>
        public virtual IList<TLog> GetLogByIds(uint[] logIds)
        {
            if (logIds == null || logIds.Length == 0)
                return new List<TLog>();

            var query = from l in _logRepository.Table
                        where logIds.Contains(l.Id)
                        select l;
            var logItems = query.ToList();
            //sort by passed identifiers
            var sortedLogItems = new List<TLog>();
            foreach (var id in logIds)
            {
                var log = logItems.Find(x => x.Id == id);
                if (log != null)
                    sortedLogItems.Add(log);
            }
            return sortedLogItems;
        }

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="shortMessage">The short message</param>
        /// <param name="fullMessage">The full message</param>
        /// <param name="user">The customer to associate log record with</param>
        /// <returns>A log item</returns>
        public virtual async Task<TLog> InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", TUser user = null, EndPoint endPoing = null, string pageUrl = null, string referrerUrl = null)
        {
            //check ignore word/phrase list?
            if (IgnoreLog(shortMessage) || IgnoreLog(fullMessage))
                return null;

            var log = CreateLogEntity(logLevel, shortMessage, fullMessage, user, DateTime.Now, (endPoing as IPEndPoint)?.Address.ToString(), pageUrl, referrerUrl);

            await _logRepository.InsertAsync(log, false);

            return log;
        }

        protected virtual TLog CreateLogEntity(LogLevel level, string shortMessage, string fullMessage, TUser user, DateTime createdTime, string ipaddress, string pageUrl = null, string referrerUrl = null)
        {
            return new TLog
            {
                LogLevel = level,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                User = user,
                CreatedTime = createdTime,
                IpAddress = ipaddress,
                PageUrl = pageUrl,
                ReferrerUrl = referrerUrl,
            };
        }

        #endregion
    }
}
