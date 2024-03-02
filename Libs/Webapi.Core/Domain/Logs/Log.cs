using System;

namespace Webapi.Core.Domain.Logs
{

    public abstract class Log : BaseEntity, ILog
    {
        ///// <summary>
        ///// Gets or sets the log level identifier
        ///// </summary>
        //public int LogLevelId { get; set; }

        /// <summary>
        /// Gets or sets the short message
        /// </summary>
        public string ShortMessage { get; set; }

        /// <summary>
        /// Gets or sets the full exception
        /// </summary>
        public string FullMessage { get; set; }

        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the page URL
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the referrer URL
        /// </summary>
        public string ReferrerUrl { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the log level
        /// </summary>
        public LogLevel LogLevel { get; set; }
    }

    public abstract class UserLog<TUser> : Log, IUserLog<TUser>
        where TUser : User
    {
        public TUser User { get; set; }
        public uint UserId { get; set; }
    }
}
