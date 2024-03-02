using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using Webapi.Services.ScheduleTasks;

namespace Webapi.Server.Controllers
{
    [Route("scheduleTask/[action]")]
    public partial class ScheduleTaskController : ControllerBase
    {
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IScheduleTaskRunner _taskRunner;

        public ScheduleTaskController(IScheduleTaskService scheduleTaskService,
            IScheduleTaskRunner taskRunner)
        {
            _scheduleTaskService = scheduleTaskService;
            _taskRunner = taskRunner;
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> RunTask([Required] string taskType)
        {
            var clientAddress = HttpContext.Connection.RemoteIpAddress;
            var localAddress = HttpContext.Connection.LocalIpAddress;
            if (clientAddress != null && localAddress != null && !localAddress.Equals(clientAddress)) 
                return NotFound();

            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(taskType);
            if (scheduleTask == null)
                //schedule task cannot be loaded
                return NoContent();

            await _taskRunner.ExecuteAsync(scheduleTask);

            return NoContent();
        }
    }
}
