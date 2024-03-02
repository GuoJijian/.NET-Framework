using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Configuration;
using Webapi.Core.Domain;
using Webapi.Core.Infrastructure;
using Webapi.Data;
using Webapi.Data.Services;
using Webapi.Framework.Logging;
using Webapi.Services.ScheduleTasks;

namespace Webapi.Server
{
    public static class ApplicationBuilderExtensions
    {

        public static async Task StartEngine(this IApplicationBuilder application, ITypeFinder typeFinder, IConfiguration configuration, AppSettings appSettings)
        {
            application.UseExceptionHandler(appSettings);
            application.UseWebSockets(appSettings);

            var startupConfigurations = typeFinder.FindClassesOfType<IWebStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                .Select(startup => Activator.CreateInstance(startup))
                .Cast<IWebStartup>()
                .OrderBy(startup => startup.Order);

            //configure request pipeline
            foreach (var instance in instances)
                instance.Configure(application, appSettings);

            var applicationServices = application.ApplicationServices;
            {
                using (var scope = applicationServices.CreateScope())
                {
                    //initialize EF DBContext
                    var innerServiceProvider = scope.ServiceProvider;
                    var dbContext = innerServiceProvider.GetRequiredService<IDbContext>();
                    var adminRepo = innerServiceProvider.GetRequiredService<IRepository<Admin>>();
                    if (dbContext.EnsureCreated() || adminRepo.TableNoTracking.Count() == 0)
                    {
                        var settingService = innerServiceProvider.GetRequiredService<ISettingService>();
                        await settingService.SaveSettingAsync(new CommonSettings());
                    }
                    dbContext.TryMigrate();

                    //initialize redis cache
                    var distributedCacheConfig = appSettings.Get<DistributedCacheConfig>();
                    if (distributedCacheConfig.Enabled && distributedCacheConfig.DistributedCacheType == DistributedCacheType.Redis)
                    {
                        var redisCache = innerServiceProvider.GetRequiredService<IRedisDistributedCache>();
                        var _ = await redisCache.GetAsync("a");
                        var keys = await redisCache.Keys("DataProtection").ToArrayAsync();
                        //await redisCache.RemoveByPrefixAsync("DataProtection");
                        //await redisCache.RemoveByPrefixAsync(null);                        
                    }
                }
            }
            var commonConfig = appSettings.Get<CommonConfig>();
            var url = (configuration["urls"] ?? "http://localhost:80/").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).First();
            var uri = new Uri(url);
            commonConfig.DefaultSiteUrl = $"{uri.Scheme}://127.0.0.1:{uri.Port}/";
            IocEngine.RunStartupTasks(typeFinder);
            //further actions are performed only when the database is installed
            if (DataSettingsManager.IsDatabaseInstalled())
            {
                using (var scope = applicationServices.CreateScope())
                {
                    var innerServiceProvider = scope.ServiceProvider;
                    //log application start
                    await innerServiceProvider.GetRequiredService<ILogger>().InformationAsync("Application started");

                    //update core and db
                    //var migrationManager = IocEngine.Resolve<IMigrationManager>();
                    //var assembly = Assembly.GetAssembly(typeof(ApplicationBuilderExtensions));
                    //migrationManager.ApplyUpMigrations(assembly, MigrationProcessType.Update);
                    //assembly = Assembly.GetAssembly(typeof(IMigrationManager));
                    //migrationManager.ApplyUpMigrations(assembly, MigrationProcessType.Update);

                    var taskScheduler = innerServiceProvider.GetRequiredService<ITaskScheduler>();
                    await taskScheduler.InitializeAsync();
                    taskScheduler.StartScheduler();
                }
            }
        }

        public static void UseWebSockets(this IApplicationBuilder application, AppSettings appSettings)
        {
            var config = appSettings.Get<WebSocketsConfig>();
            if (config.UseWebSockets)
            {
                application.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(10) });
                application.Use(async (ct, next) =>
                {
                    if (ct.WebSockets.IsWebSocketRequest && ct.Request.Path == config.WebSocketRequestPath)
                    {
                        var handler = ct.RequestServices.GetRequiredService<IWebSocketHandler>();
                        await handler.HandleWebSocket(ct, await ct.WebSockets.AcceptWebSocketAsync(), config);
                    }
                    else
                    {
                        await next(ct);
                    }
                });
            }
        }

        public static void UseExceptionHandler(this IApplicationBuilder application, AppSettings appSettings)
        {
            application.UseExceptionHandler(handler =>
            {
                handler.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (exception == null)
                        return;

                    try
                    {
                        //log error
                        var pageUrl = context.Request.Path;
                        var referrerUrl = context.Request.Headers.Referer;
                        var ep = context.Connection.RemoteIpAddress != null ? new IPEndPoint(context.Connection.RemoteIpAddress, context.Connection.RemotePort) : null;
                        await context.RequestServices.GetRequiredService<ILogger>().ErrorAsync(exception.Message, exception, ep, pageUrl, referrerUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ExceptionHandler, ex: {ex};{Environment.NewLine}ex2:{exception};");
                    }
                    finally
                    {
#if DEBUG
                        //rethrow the exception to show the error page
                        ExceptionDispatchInfo.Throw(exception);
#endif
                    }
                });
            });
        }

        public static void RunFirstTasks(this IApplicationBuilder application, IConfiguration configuration, AppSettings appSettings)
        {

        }

        /// <summary>
        /// Configure middleware for dynamically compressing HTTP responses
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void CheckResponseCompression(this IApplicationBuilder application)
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //whether to use compression (gzip by default)
            if (application.ApplicationServices.GetRequiredService<CommonSettings>().UseResponseCompression)
                application.UseResponseCompression();
        }

        /// <summary>
        /// Configure middleware checking whether requested page is keep alive page
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        //public static void UseKeepAlive(this IApplicationBuilder application)
        //{
        //    application.UseMiddleware<KeepAliveMiddleware>();
        //}

    }
}
