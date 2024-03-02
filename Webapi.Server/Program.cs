using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Infrastructure;
using Webapi.Server.Controllers;
using Webapi.Server.IoC;

namespace Webapi.Server
{
    public class Program
    {
        static IHost Host;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Webapi server starting......");

            await StartWebApplication(ServiceConfig);

            bool exit = false;
            var reader = new StreamReader(Console.OpenStandardInput());
            Action action = null;
            action = () =>
            {
                Console.WriteLine("Type 'quit' or 'exit' and press Enter to stop the Webapi server.");
                reader.ReadLineAsync().ContinueWith(task => parseCmd(task.Result, action, ref exit));
            };
            action();
            while (!exit)
            {
                //TODO 
                Thread.Sleep(500);
            }
            await Host.StopAsync();
        }

        public static async Task StartWebApplication(Action<IServiceCollection, AppSettings> serverConfig = null)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonFile(NopConfigurationDefaults.AppSettingsFilePath, true, true);
            builder.Configuration.AddEnvironmentVariables();
            var (typeFinder, appSettings) = builder.Services.ConfigureApplicationServices(builder);
            if (serverConfig != null)
            {
                serverConfig(builder.Services, appSettings);
            }

#if DEBUG
            var excludeControllers = new[] { typeof(ScheduleTaskController).Name, typeof(Keepalive).Name };
            builder.Services.AddSwaggerGen(options =>
            {
                options.DocInclusionPredicate((docname, apiDesc) =>
                {
                    var desc = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    if (desc != null)
                    {
                        if (excludeControllers.Any(name => name.Equals(desc.ControllerTypeInfo.Name)))
                        {
                            return false;
                        }
                    }
                    return true;
                });
            });
#endif
            builder.Services.AddWebApiHttpClients();
            builder.Services.AddWebapiAuthentication(appSettings);

            builder.Host.UseServiceProviderFactory(hostBuilderContext => new DefaultServiceProviderFactory(typeFinder, builder.Configuration, appSettings, configurationAction));

            var app = builder.Build();
/*            //use HTTP session
            app.UseSession();*/

            await app.StartEngine(typeFinder, builder.Configuration, appSettings);
            app.UseW3CLogging();
            var commonConfig = appSettings.Get<CommonConfig>();
            if (commonConfig.UseHttpDebugLogging)
            {
                app.UseHttpLogging();
            }
            app.UseCors();
            app.UseStaticFiles(appSettings); 

#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI();
#endif

            //使用身份认证
            app.UseAuthentication();
            //使用身份授权
            app.UseAuthorization();
            app.CheckResponseCompression();
            app.MapControllers();

            Host = app;
            await app.StartAsync();
            app.RunFirstTasks(builder.Configuration, appSettings);
        }

        static IContainer configurationAction(ContainerBuilder containerBuilder, ITypeFinder typeFinder, IConfiguration configuration, AppSettings appSettings)
        {
            var iocEngine = new HttpIocEngine();
            var container = iocEngine.RegisterDependencies(containerBuilder, typeFinder, configuration, appSettings);
            IocEngine.SetEngine(iocEngine);
            return container;
        }

        static void parseCmd(string line, Action action, ref bool exit)
        {
            if (line != null)
            {
                var cmd = line.ToLower();
                switch (cmd)
                {
                    case "quit":
                    case "exit":
                        Console.WriteLine("正在退出......");
                        exit = true;
                        return;
                    default:
                        break;
                }
                Console.WriteLine("无法识别的命令：{0}", cmd);
            }
            else
            {
                Console.WriteLine("无效输入！");
            }
            action();
        }

        static void ServiceConfig(IServiceCollection services, AppSettings appSettings)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AddServerHeader = false;
                var httpsConfig = appSettings.Get<HttpsConfig>();
                if (httpsConfig.UseHttps)
                {
                    var certificate = new X509Certificate2(httpsConfig.Certificate2FileName, httpsConfig.Password);
                    options.ConfigureHttpsDefaults(p =>
                    {
                        p.ServerCertificate = certificate;
                        p.SslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
                    });
                }
            });
        }

    }
}
