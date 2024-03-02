using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Caching;
using Webapi.Core.Domain;
using Webapi.Core.Domain.Logs;
using Webapi.Data;
using Webapi.Framework.Logging;
using Webapi.Services.Caching;

namespace Webapi.Admins.Main.Service
{
    class AdminCacheEventConsumer : CacheEventConsumer<Admin>
    {
        public AdminCacheEventConsumer(IStaticCacheManager staticCacheManager)
            : base(staticCacheManager)
        {

        }
    }


    public class AdminService : IAdminService
    {

        IRepository<Admin> repository;
        ILogger<Admin, AdminLog> adminLog;
        IStaticCacheManager staticCacheManager;
        ILogger sysLog;

        public ClaimsPrincipal CurrentUser { get; }

        public AdminService(IRepository<Admin> repository, ILogger sysLog, ILogger<Admin, AdminLog> adminLog, IHttpContextAccessor accessor,IStaticCacheManager staticCacheManager)
        {
            this.repository = repository;
            this.sysLog = sysLog;
            this.adminLog = adminLog;
            this.staticCacheManager = staticCacheManager;
            CurrentUser = accessor.HttpContext.User;
        }

        Admin _admin;
        async Task<Admin> GetCurrentAdmin()
        {
            if (_admin == null)
            {
                var idstr = CurrentUser.FindFirst(ClaimTypes.NameIdentifier);
                if (idstr != null && uint.TryParse(idstr.Value, out var id))
                {
                    _admin = await repository.GetByIdAsync(id);
                }
            }
            return _admin;
        }

        public async Task<(int, Admin)> Login(string name, string password)
        {
            var admin = await repository.GetEntityAsync(p => p.Name == name && p.PasswordHash == PasswordUtil.GetPasswordHash(password));
            if (admin == null)
            {
                await sysLog.WarningAsync($"admin login failed, name: {name}, password: {password}");
                return (-100, admin);
            }
            if (admin.block)
            {
                await adminLog.WarningAsync($"admin login failed, name: {name}, the account is block!", admin);
                return (-102, null);
            }
            await adminLog.InformationAsync($"admin login success, name: {name}", admin);
            return (0, admin);
        }

        public async Task<IEnumerable<Admin>> List(bool includeDeleted)
        {
            //                                      不过滤
            var list = await repository.GetAllAsync(query => query, includeDeleted);
            return list;
        }

        public async Task<(int, Admin)> New(string name, string password)
        {
            var exists = repository.TableNoTracking.Where(p => p.Name == name).SingleOrDefault() != null;
            if (exists)
            {
                return (-110, null);
            }
            var admin = new Admin { Name = name, PasswordHash = PasswordUtil.GetPasswordHash(password), Datetime = DateTime.Now };
            try
            {
                await repository.InsertAsync(admin);
            }
            catch (Exception ex)
            {
                await adminLog.ErrorAsync($"add admin failed: name: {name}, password: {password}", await GetCurrentAdmin(), ex);
                return (-112, null);
            }
            return (0, admin);
        }

        public async Task<(int, Admin)> Edit(uint id, string name, string password)
        {
            var admin = await this.repository.GetByIdAsync(id);
            if (admin == null)
            {
                return (-120, null);
            }
            if (admin.BuildIn)
            {
                //not allow edit the admin in the mothod
                return (-124, (Admin)null);
            }

            if (!string.Equals(admin.Name, name))
            {
                if (repository.GetEntityAsync(p => p.Id != id && p.Name == name) != null)
                {
                    //user name used
                    return (-122, null);
                }
                admin.Name = name;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                //reset password
                admin.PasswordHash = PasswordUtil.GetPasswordHash(password);
            }
            try
            {
                await repository.UpdateAsync(admin);
            }
            catch (Exception ex)
            {
                await adminLog.ErrorAsync($"admin edit failed, id: {id}, name: {name}, password: {password}", await GetCurrentAdmin(), ex);
            }
            return (0, admin);
        }

        public async Task<Admin> Detail(uint id)
        {
            var admin = await this.repository.GetByIdAsync(id);
            return admin;
        }
        public async Task<IPagedList<Admin>> Paged(int pageIndex, int pageSize)
        {
            var pagedList = await staticCacheManager.GetAsync(staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<Admin>.ByPagedCacheKey), async () => await this.repository.GetAllPagedAsync(null, pageIndex, pageSize));
            return pagedList;
        }

        public async Task<int> Delete(uint id)
        {
            var admin = await this.repository.GetByIdAsync(id);
            if (admin == null)
            {
                return -130;
            }
            if (admin.BuildIn)
            {
                await adminLog.ErrorAsync($"admin delete failed, id: {id}, try delete the buildin admin", await GetCurrentAdmin());
                return -132;
            }
            await repository.DeleteAsync(admin);
            await adminLog.WarningAsync($"admin delete success, id: {id}, name: {admin.Name}", await GetCurrentAdmin());
            return 0;
        }

        public async Task<int> Block(uint id)
        {
            var admin = await this.repository.GetByIdAsync(id);
            if (admin == null)
            {
                return -140;
            }
            if (admin.block)
            {
                return -142;
            }
            if (admin.BuildIn)
            {
                await adminLog.ErrorAsync($"admin block failed, id: {id}, try block the buildin admin", await GetCurrentAdmin());
                return -144;
            }
            else
            {
                admin.block = true;
                await repository.UpdateAsync(admin);
                await adminLog.WarningAsync($"admin block success, id: {id}, name: {admin.Name}", await GetCurrentAdmin());
                return 0;
            }
        }


        public async Task<int> Unblock(uint id)
        {
            var admin = await this.repository.GetByIdAsync(id);
            if (admin == null)
            {
                return -150;
            }
            if (!admin.block)
            {
                return -152;
            }
            if (admin.BuildIn)
            {
                await adminLog.ErrorAsync($"admin unblock failed, id: {id}, try unblock the buildin admin", await GetCurrentAdmin());
                return -154;
            }
            else
            {
                admin.block = false;
                await repository.UpdateAsync(admin);
                await adminLog.WarningAsync($"admin unblock success, id: {id}, name: {admin.Name}", await GetCurrentAdmin());
                return 0;
            }
        }
    }
}
