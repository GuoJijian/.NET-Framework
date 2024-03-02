using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Domain;

namespace Webapi.Admins.Main
{
    public interface IAdminService
    {
        Task<(int, Admin)> Login(string name, string password);
        Task<IEnumerable<Admin>> List(bool includeDeleted);
        Task<(int, Admin)> New(string name, string password);
        Task<(int, Admin)> Edit(uint id, string name, string password);
        Task<Admin> Detail(uint id);
        Task<IPagedList<Admin>> Paged(int pageIndex, int pageSize);
        Task<int> Delete(uint id);
        Task<int> Block(uint id);
        Task<int> Unblock(uint id);


    }
}
