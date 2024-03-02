using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Caching;

namespace Webapi.Services.Authentication
{
    public class FileCookieTicketStore : ITicketStore, IManegedTicketStore
    {
        public FileCookieTicketStore(DirectoryInfo root, AuthenticationScheme scheme, CookieAuthenticationOptions cookieAuthenticationOptions)
        {
            Scheme = scheme;
            CookieAuthenticationOptions = cookieAuthenticationOptions;
            cookieDirectory = new DirectoryInfo(Path.Combine(root.FullName, Scheme.Name));
            if (!cookieDirectory.Exists)
            {
                cookieDirectory.Create();
            }
        }
        public DirectoryInfo cookieDirectory { get; }
        public AuthenticationScheme Scheme { get; }
        public CookieAuthenticationOptions CookieAuthenticationOptions { get; }

        #region ITicketStore Method
        public Task<AuthenticationTicket> RetrieveAsync(string rawkey)
        {
            return RetrieveAsync(rawkey, Scheme.Strictly);
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var id = ticket.Principal.Claims.First(p => p.Type == ClaimTypes.NameIdentifier).Value;
            var tag = GetTag();
            await WriteFile(id, ticket, tag);
            return $"{id}_{tag}";
        }

        public async Task RenewAsync(string rawkey, AuthenticationTicket ticket)
        {
            var (succ, fileName, tag) = ParseKey(rawkey);
            if (succ)
            {
                await WriteFile(fileName, ticket, tag);
            }
        }

        public Task RemoveAsync(string rawkey)
        {
            var (succ, fileName, tag) = ParseKey(rawkey);
            if (succ)
            {
                var fileInfo = GetFileInfo(fileName);
                if (fileInfo.Exists)
                {
                    try
                    {
                        fileInfo.Delete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"FileCookieTicketStore, scheme: {Scheme.Name}, RemoveAsync, ex: {ex}");
                    }
                }
            }
            return Task.CompletedTask;
        }
        #endregion

        FileInfo GetFileInfo(string key) => new FileInfo(Path.Combine(cookieDirectory.FullName, key));

        async Task<AuthenticationTicket> RetrieveAsync(string rawkey, bool strictly)
        {
            var (succ, fileName, tag) = ParseKey(rawkey);
            if (succ)
            {
                var (succ2, ticketData, tag1) = await ReadFile(fileName);
                if (succ2)
                {
                    if (strictly && !tag.Equals(tag1)) return null;
                    return CookieAuthenticationOptions.TicketDataFormat.Unprotect(ticketData);
                }
            }
            return null;
        }


        #region IManegedTicketStore
        public async Task StoreAsync(uint userId, AuthenticationTicket ticket)
        {
            var (succ, _, tag) = await ReadFile(userId.ToString());
            if (succ)
            {
                await WriteFile(userId.ToString(), ticket, tag);
            }
        }

        public Task<AuthenticationTicket> RetrieveAsync(uint userId)
        {
            return RetrieveAsync($"{userId}_0", false);
        }

        public Task RemoveAsync(uint userId)
        {
            return RemoveAsync($"{userId}_0");
        }
        #endregion

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
            var fileName = array[0];
            var tag = array[1];
            return (true, fileName, tag);
        }

        async Task<(bool, string, string)> ReadFile(string fileName)
        {
            var fileInfo = GetFileInfo(fileName);
            if (fileInfo.Exists && fileInfo.Length > 0)
            {
                try
                {
                    var lines = await File.ReadAllLinesAsync(fileInfo.FullName, Encoding.UTF8);
                    if (lines.Length == 2)
                    {
                        return (true, lines[0], lines[1]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FileCookieTicketStore, scheme: {Scheme.Name}, ReadFile, ex: {ex}");
                }
            }
            return (false, null, null);
        }

        async Task WriteFile(string filename, AuthenticationTicket ticket, string tag)
        {
            try
            {
                var fileInfo = GetFileInfo(filename);
                var ticketData = CookieAuthenticationOptions.TicketDataFormat.Protect(ticket);
                await File.WriteAllLinesAsync(fileInfo.FullName, new[] { ticketData, tag }, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FileCookieTicketStore, scheme: {Scheme.Name}, WriteFile, ex: {ex}");
            }
        }
    }
}

