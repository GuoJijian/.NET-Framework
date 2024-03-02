using Webapi.Core;
using System;
using System.Collections.Generic;
using System.Net;
using Webapi.Core.Domain.Logs;
using System.Threading.Tasks;
using Webapi.Data;
using Webapi.Core.Configuration;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Webapi.Framework.Logging
{
    public class Logger : ILogger
    {
        public Logger(IRepository<SysLog> repository, CommonSettings commonSettings)
        {
            _logRepository = repository;
            _commonSettings = commonSettings;
        }

        IRepository<SysLog> _logRepository;
        CommonSettings _commonSettings;
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
        public virtual bool IsEnabled(LogLevel level)
        {
            return (this._commonSettings.LogLevel > LogLevel.Debug && level >= this._commonSettings.LogLevel) || level > LogLevel.Debug;
        }

        public async Task DeleteLog(SysLog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            await _logRepository.DeleteAsync(log);
        }

        public async Task DeleteLogs(IList<SysLog> logs)
        {
            if (logs == null)
                throw new ArgumentNullException(nameof(logs));

            await _logRepository.DeleteAsync(logs);
        }

        public async Task ClearLog()
        {
            await _logRepository.TruncateAsync();
        }

        public async Task<IPagedList<SysLog>> GetAllLogsAsync(DateTime? fromTime = null, DateTime? toTime = null, string message = "", LogLevel? logLevel = null, int pageIndex = 0, int pageSize = int.MaxValue)
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

            var plog = await query.ToPagedListAsync(pageIndex, pageSize);
            return plog;
        }

        public async Task<SysLog> GetLogById(uint logId)
        {
            if (logId == 0)
                return null;

            return await _logRepository.GetByIdAsync(logId);
        }

        public async Task<IList<SysLog>> GetLogByIds(uint[] logIds)
        {
            if (logIds == null || logIds.Length == 0)
                return new List<SysLog>();

            var query = from l in _logRepository.Table
                        where logIds.Contains(l.Id)
                        select l;
            var logItems = await query.OrderBy(p => p.Id).ToArrayAsync();
            return logItems;
        }

        public async Task<SysLog> InsertLog(LogLevel logLevel, string shortMessage, string fullMessage = "", EndPoint endPoing = null, string pageUrl = null, string referrerUrl = null)
        {
            if (IgnoreLog(shortMessage) || IgnoreLog(fullMessage))
                return null;

            var log = CreateLogEntity(logLevel, shortMessage, fullMessage, DateTime.Now, (endPoing as IPEndPoint)?.Address.ToString(), pageUrl, referrerUrl);

            await _logRepository.InsertAsync(log);

            return log;
        }

        protected virtual SysLog CreateLogEntity(LogLevel level, string shortMessage, string fullMessage, DateTime createdTime, string endPoing, string pageUrl = null, string referrerUrl = null)
        {
            return new SysLog
            {
                LogLevel = level,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                CreatedTime = createdTime,
                IpAddress = endPoing,
                PageUrl = pageUrl,
                ReferrerUrl = referrerUrl
            };
        }

        public async Task ClearLogAsync(DateTime? olderThan = null)
        {
            if (olderThan == null)
                await _logRepository.TruncateAsync();
            else
                await _logRepository.DeleteAsync(p => p.CreatedTime < olderThan.Value);
        }
    }
}
