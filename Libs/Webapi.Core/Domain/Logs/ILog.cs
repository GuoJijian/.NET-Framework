using System;

namespace Webapi.Core.Domain.Logs
{
    public interface IUserLog<TUser> : ILog where TUser : User
    {
        TUser User { get; set; }
        uint UserId { get; set; }
    }

    public interface ILog
    {
        DateTime CreatedTime { get; set; }
        string FullMessage { get; set; }
        string IpAddress { get; set; }
        LogLevel LogLevel { get; set; }
        string PageUrl { get; set; }
        string ReferrerUrl { get; set; }
        string ShortMessage { get; set; }
    }
}