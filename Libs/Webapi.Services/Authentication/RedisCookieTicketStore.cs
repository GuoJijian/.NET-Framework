using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Caching;
using Webapi.Core.Infrastructure;
using Webapi.Data;

namespace Webapi.Services.Authentication
{
    public interface IManegedTicketStoreDictionary : IDictionary<string, IManegedTicketStore> { }
    public class ManegedTicketStoreDictionary : Dictionary<string, IManegedTicketStore>, IManegedTicketStoreDictionary
    {
    }

    public interface IManegedTicketStore
    {
        Task StoreAsync(uint userId, AuthenticationTicket ticket);
        Task<AuthenticationTicket> RetrieveAsync(uint userId);
        Task RemoveAsync(uint userId);
    }

    public class RedisCookieTicketStore : ITicketStore, IManegedTicketStore
    {
        public RedisCookieTicketStore(IStaticCacheManager staticCacheManager, AuthenticationScheme scheme, CookieAuthenticationOptions cookieAuthenticationOptions)
        {
            StaticCacheManager = staticCacheManager;
            Scheme = scheme;
            CookieAuthenticationOptions = cookieAuthenticationOptions;
        }

        public IStaticCacheManager StaticCacheManager { get; }
        public AuthenticationScheme Scheme { get; }
        public CookieAuthenticationOptions CookieAuthenticationOptions { get; }

        private const string KeyPrefix = "AuthSessionStore";

        CacheKey getKey(string id) => new CacheKey($"{KeyPrefix}.{Scheme.Name}.{id}") { CacheTime = (int)Scheme.ExpireTimeSpan.TotalMinutes };

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var id = ticket.Principal.Claims.First(p => p.Type == ClaimTypes.NameIdentifier).Value;
            var guid = GetTag();
            await RenewAsync(id, ticket, guid);
            return $"{id}_{guid}";
        }

        public async Task RenewAsync(string rawkey, AuthenticationTicket ticket)
        {
            var (succ, key, tag) = ParseKey(rawkey);
            if (succ)
            {
                await RenewAsync(key, ticket, tag);
            }
        }

        async Task RenewAsync(string key, AuthenticationTicket ticket, string tag)
        {
            var ticketData = CookieAuthenticationOptions.TicketDataFormat.Protect(ticket);
            await StaticCacheManager.SetAsync(getKey(key), (ticketData, tag));
        }

        public Task<AuthenticationTicket> RetrieveAsync(string rawkey)
        {
            return RetrieveAsync(rawkey, Scheme.Strictly);
        }

        async Task<AuthenticationTicket> RetrieveAsync(string rawkey, bool strictly)
        {
            var (succ, key, tag) = ParseKey(rawkey);
            if (succ)
            {
                var (ticketData, tag1) = await StaticCacheManager.GetAsync<(string, string)>(getKey(key), null);
                if (strictly && tag != tag1) return null;
                return CookieAuthenticationOptions.TicketDataFormat.Unprotect(ticketData);
            }
            return null;
        }

        public async Task RemoveAsync(string rawkey)
        {
            var (succ, key, _) = ParseKey(rawkey);
            if (succ)
            {
                await StaticCacheManager.RemoveAsync(getKey(key));
            }
        }

        public async Task StoreAsync(uint userId, AuthenticationTicket ticket)
        {
            var key = getKey(userId.ToString());
            var (ticketData, tag) = await StaticCacheManager.GetAsync<(string, string)>(key, () => null);
            tag ??= GetTag();
            ticketData = CookieAuthenticationOptions.TicketDataFormat.Protect(ticket);
            await StaticCacheManager.SetAsync(key, (ticketData, tag));
        }

        public Task<AuthenticationTicket> RetrieveAsync(uint userId)
        {
            return RetrieveAsync($"{userId}_0", false);
        }

        public Task RemoveAsync(uint userId)
        {
            return RemoveAsync($"{userId}_0");
        }

        string GetTag() => Guid.NewGuid().ToString("N");

        (bool, string, string) ParseKey(string key)
        {
            if (!key.Contains('_'))
            {
                return (false, null, null);
            }
            var array = key.Split("_");
            if (array.Length != 2)
            {
                return (false, null, null);
            }
            return (true, array[0], array[1]);
        }
    }
}

