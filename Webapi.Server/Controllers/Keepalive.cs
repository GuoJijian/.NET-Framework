using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Webapi.Core.Common;

namespace Webapi.Server.Controllers
{
    [Route("/keepalive/[action]")]
    [AllowAnonymous]
    public class Keepalive : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            var clientAddress = HttpContext.Connection.RemoteIpAddress;
            var localAddress = HttpContext.Connection.LocalIpAddress;
            if (clientAddress != null && localAddress != null && !localAddress.Equals(clientAddress)) 
                return NotFound();

            return Ok();
        }

#if DEBUG
        [HttpGet]
        public IActionResult Ex()
        {
            throw new System.Exception();
        }
#endif
    }
}
