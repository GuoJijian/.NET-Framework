using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Webapi.Core.Domain;

namespace Webapi.Data
{
    public static class UserExtensions
    {
        public static async Task<Admin> GetCurrentAdmin(this Admin admin, IRepository<Admin> repositoryAdmin, ClaimsPrincipal currentUser)
        {
            if (admin == null)
            {
                var idstr = currentUser.FindFirst(ClaimTypes.NameIdentifier);
                if (idstr != null && uint.TryParse(idstr.Value, out var id))
                {
                    admin = await repositoryAdmin.Table.SingleAsync(p => p.Id == id);
                }
            }
            return admin;
        }




    }



}
