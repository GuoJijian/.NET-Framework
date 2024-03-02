using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webapi.Core {
    /// <summary>
    /// Interface which should be implemented by tasks run on startup
    /// </summary>
    public interface IStartupTask
    {
        /// <summary>
        /// 执行一个任务
        /// </summary>
        void Execute();

        /// <summary>
        /// 启动任务的顺序
        /// </summary>
        int Order { get; }
    }
}
