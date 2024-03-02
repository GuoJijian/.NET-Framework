using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Webapi.Admins.Main.Service;
using Webapi.Core;
using Webapi.Core.Domain;
using Webapi.Core.Domain.Logs;
using Webapi.Data;
using Webapi.Framework;
using Webapi.Services.Authentication;

namespace Webapi.Admins.Main
{
    [Area("/api/admin")]
    [Authorize(AuthenticationSchemes = AdminAuthenticationScheme)]
    [Route("[area]/[controller]/[action]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class AccountController : ControllerBase
    {
        public const string AdminAuthenticationScheme = "admin";
        [AllowAnonymous]
        [HttpGet]
        public async Task<object> Login([FromServices] IAdminService adminService, [Required] string name, [Required] string password)
        {
            var (code, admin) = await adminService.Login(name, password);
            if (code != 0)
            {
                return new { code };
            }

            var identity = new ClaimsIdentity(AdminAuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()));
            if (!string.IsNullOrEmpty(admin.Roles))
            {
                foreach (var role in admin.Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
            if (admin.BuildIn)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, AuthorizeDescription.buildinAdmin));
            }
            identity.AddClaim(new Claim(ClaimTypes.Name, name));
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(AdminAuthenticationScheme, principal);
            return new { code, admin };
        }

        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin},{AuthorizeDescription.userList}")]
        [HttpGet]
        public async Task<object> List([FromServices] IAdminService adminService, [FromServices] IWebHelper webHelper, [Required] bool includeDeleted)
        {
            //var currentUrl = webHelper.GetThisPageUrl(false);
            //var storeHost = webHelper.GetStoreHost(false);
            //var rawUrl = webHelper.GetRawUrl(Request);
            //var ip = webHelper.GetCurrentIpAddress();

            var canManage = User.HasClaim(ClaimTypes.Role, AuthorizeDescription.buildinAdmin) ||
                            User.HasClaim(ClaimTypes.Role, AuthorizeDescription.userManage);

            var list = await adminService.List(includeDeleted);

            return new { code = 0, canManage, list };
        }

        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin},{AuthorizeDescription.userManage}")]
        [HttpGet]
        public async Task<object> New([FromServices] IAdminService adminService, [Required] string name, [Required] string password)
        {

            var (code, admin) = await adminService.New(name, password);
            if (code != 0)
            {
                return new { code };
            }
            return new { code, admin };
        }

        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin},{AuthorizeDescription.userManage}")]
        [HttpGet]
        public async Task<object> Edit([FromServices] IAdminService adminService, [Range(1, uint.MaxValue)] uint id, [Required] string name, [Required] string password)
        {

            var (code, admin) = await adminService.Edit(id, name, password);
            if (code != 0)
            {
                return new { code };
            }
            return new { code, admin };
        }

        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin},{AuthorizeDescription.userList}")]
        [HttpGet]
        public async Task<object> Detail([FromServices] IAdminService adminService, [Range(1, uint.MaxValue)] uint id)
        {
            var admin = await adminService.Detail(id);

            return new { code = admin == null ? -150 : 0, admin };
        }

        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin},{AuthorizeDescription.userList}")]
        [HttpGet]
        public async Task<object> Paged([FromServices] IAdminService adminService, [Range(0, uint.MaxValue)] int pageIndex, [Range(5, 30)] int pageSize)
        {
            var list = await adminService.Paged(pageIndex, pageSize);
            return new { code = 0, list };
        }

        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin},{AuthorizeDescription.userManage}")]
        [HttpGet]
        public async Task<object> Delete([FromServices] IManegedTicketStoreDictionary manegedTicketStore,
            [FromServices] IAdminService adminService, [Range(1, uint.MaxValue)] uint id)
        {
            var code = await adminService.Delete(id);
            if (code == 0)
            {
                if (manegedTicketStore.TryGetValue(AdminAuthenticationScheme, out var ticketStore))
                {
                    await ticketStore.RemoveAsync(id);
                }
            }
            return new { code };
        }

        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin},{AuthorizeDescription.userManage}")]
        [HttpGet]
        public async Task<object> Block([FromServices] IAdminService adminService,
            [FromServices] IManegedTicketStoreDictionary manegedTicketStore, [Range(1, uint.MaxValue)] uint id)
        {

            var code = await adminService.Block(id);
            if (code != 0)
            {
                return new { code };
            }
            if (manegedTicketStore.TryGetValue(AdminAuthenticationScheme, out var ticketStore))
            {
                await ticketStore.RemoveAsync(id);
            }
            return new { code };
        }

        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin},{AuthorizeDescription.userManage}")]
        [HttpGet]
        public async Task<object> Unblock([FromServices] IAdminService adminService, [Range(1, uint.MaxValue)] uint id)
        {
            var code = await adminService.Unblock(id);
            return new { code };
        }



        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin},{AuthorizeDescription.userManage}")]
        [HttpGet]
        public object GetRoleDescription()
        {
            var type = typeof(AuthorizeDescription);
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(p => new { type = p, desc = p.GetCustomAttributes<DescriptionAttribute>().SingleOrDefault() }).Where(p => p.desc != null)
                .ToArray();

            var roleDescription = fields.ToDictionary(p => p.type.Name, p => p.desc.Description);

            return new { code = 0, roleDescription };
        }


        [HttpGet]
        public async Task<object> Logout()
        {
            await HttpContext.SignOutAsync(AdminAuthenticationScheme);
            return new { code = 0 };
        }


        [Authorize(Roles = $"{AuthorizeDescription.buildinAdmin}")]
        [HttpGet]
        public IActionResult RestartApp([FromServices] IWebHelper webHelper)
        {
            webHelper.RestartAppDomain();
            return Ok();
        }
    }
}
