using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Webapi.Core;
using Webapi.Core.Configuration;
using Webapi.Core.Http;
using Webapi.Core.Infrastructure;
using Webapi.Data;
using Webapi.Data.Infrastructure;
using Webapi.Services.Authentication;
using Webapi.Core.Common;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Hosting.Server;
using Webapi.Server.Filters;
using Microsoft.Extensions.DependencyModel;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Serialization;
using StackExchange.Redis;
using Microsoft.AspNetCore.HttpLogging;
using Webapi.Core.Logging.FileLogger;
using System.Net.Http;
using Microsoft.Extensions.Logging.Console;

namespace Webapi.Server
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add services to the application and configure service provider
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="builder">A builder for web applications and services</param>
        public static (ITypeFinder, AppSettings) ConfigureApplicationServices(this IServiceCollection services,
            WebApplicationBuilder builder)
        {
            //let the operating system decide what TLS protocol version to use
            //see https://docs.microsoft.com/dotnet/framework/network-programming/tls
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

            //create default file provider
            CommonHelper.DefaultFileProvider = new FileProvider(builder.Environment);

            var typeFinder = new WebAppTypeFinder();
            services.AddSingleton<ITypeFinder>(typeFinder);
            var configurations = typeFinder
                .FindClassesOfType<IConfig>()
                .Select(configType => (IConfig)Activator.CreateInstance(configType))
                .ToList();
            foreach (var config in configurations)
            {
                builder.Configuration.GetSection(config.Name).Bind(config, options => options.BindNonPublicProperties = true);
                services.AddSingleton(config.GetType(), config);
            }
            var authConfig = configurations.OfType<AuthenticationConfig>().Single();
            if (!authConfig.Any())
            {
                authConfig.Add(new Services.Authentication.AuthenticationScheme
                {
                    Name = "admin",
                    SlidingExpiration = true,
                    ExpireTimeSpan = TimeSpan.FromDays(30),
                });
            }
            var appSettings = AppSettingsHelper.SaveAppSettings(configurations, CommonHelper.DefaultFileProvider, false);

            var commonConfig = appSettings.Get<CommonConfig>();
            if (commonConfig.Urls.Count == 0)
            {
                commonConfig.Urls.AddRange(new[] { "http://0.0.0.0:80", "http://*:8080", "http://*:5014", "https://*:443" });
                appSettings = AppSettingsHelper.SaveAppSettings(configurations, CommonHelper.DefaultFileProvider, true);
            }

            if (commonConfig.AddResponseCompressionService)
            {
                services.AddResponseCompression();
            }

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            if (commonConfig.UseHttpDebugLogging)
            {
                services.AddHttpLogging(option =>
                {
                    option.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
                });
            }

            builder.Logging.AddFile();
            builder.Logging.AddW3CLogging();
            builder.WebHost.UseUrls(commonConfig.Urls.ToArray());
            services.AddSingleton(appSettings);
            //add accessor to HttpContext
            services.AddHttpContextAccessor();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowCredentials()
                    //.AllowAnyOrigin()
                    .AllowAnyHeader()
                    ;
                });
            });

            var mvcCoreBuilder = services.AddMvcCore(options =>
            {
                options.Filters.Add<ActionParameterFilter>();
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            })
            .AddApiExplorer();
            services.AddControllers();

            var applicationParts = mvcCoreBuilder.PartManager.ApplicationParts;
            var defaultdeps = DependencyContext.Default;
            var controllerType = typeof(ControllerBase);
            var defaultdepsByProject = defaultdeps.CompileLibraries.Where(p => p.Type == "project").ToArray();
            foreach (var lib in defaultdepsByProject.Where(p => !p.Name.Contains("Test")))
            {
                var assembly = Assembly.Load(lib.Name);
                if (assembly.ExportedTypes.Any(p => controllerType.IsAssignableFrom(p)))
                {
                    applicationParts.Add(new AssemblyPart(assembly));
                }
            }

            services.AddDataProtection(options => options.ApplicationDiscriminator = "default").AddPersistKeys(appSettings);
            if (commonConfig.UseSessionStateTempDataProvider)
            {
                services.AddHttpSession();
            }

            //web helper
            services.AddScoped<IWebHelper, WebHelper>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            var startupConfigurations = typeFinder.FindClassesOfType<IWebStartup>();
            //create and sort instances of startup configurations
            var instances = startupConfigurations
                .Select(startup => Activator.CreateInstance(startup))
                .Cast<IWebStartup>()
                .OrderBy(startup => startup.Order);

            //configure services
            foreach (var instance in instances)
                instance.ConfigureServices(services, builder.Configuration, typeFinder, appSettings);

            services.AddWebSockets(appSettings);
            return (typeFinder, appSettings);
        }

        public static IDataProtectionBuilder AddPersistKeys(this IDataProtectionBuilder builder, AppSettings appSettings)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            var distributedCacheConfig = appSettings.Get<DistributedCacheConfig>();
            builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
            {
                var loggerFactory = services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
                return new ConfigureOptions<KeyManagementOptions>(options =>
                {
                    options.NewKeyLifetime = TimeSpan.FromDays(30 * 366);
                    if (distributedCacheConfig.Enabled && distributedCacheConfig.DistributedCacheType == DistributedCacheType.Redis)
                    {
                        options.XmlRepository = new DistributedCacheXmlRepository(services.GetRequiredService<IStaticCacheManager>(), services.GetRequiredService<IRedisDistributedCache>(), loggerFactory);
                    }
                    else
                    {
                        options.XmlRepository = new FileSystemXmlRepository(NopConfigurationDefaults.AppSettingsDirectory, loggerFactory);
                    }
                });
            });

            return builder;
        }

        /// <summary>
        /// Register HttpContextAccessor
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        /// <summary>
        /// Adds services required for anti-forgery support
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddAntiForgery(this IServiceCollection services)
        {
            //override cookie name
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = $"{NopCookieDefaults.Prefix}{NopCookieDefaults.AntiforgeryCookie}";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
        }

        /// <summary>
        /// Adds services required for application session state
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddHttpSession(this IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.Cookie.Name = $"{NopCookieDefaults.Prefix}{NopCookieDefaults.SessionCookie}";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });
        }

        public static void UseStaticFiles(this IApplicationBuilder application, AppSettings appSettings)
        {
            var commonConfig = appSettings.Get<CommonConfig>();
            if (commonConfig.UseStaticFiles)
            {
                var staticFileDir = new DirectoryInfo(commonConfig.StaticFilesRoot);
                if (!staticFileDir.Exists)
                {
                    staticFileDir.Create();
                }

                //common static files
                var contentTypeProvider = new FileExtensionContentTypeProvider();
                if (commonConfig.ContentTypeMappings.Any())
                {
                    foreach (var item in commonConfig.ContentTypeMappings)
                    {
                        if (!contentTypeProvider.TryGetContentType(item.Key, out var _))
                        {
                            contentTypeProvider.Mappings.Add(item.Key, item.Value);
                        }
                    }
                }
#if DEBUG
                var fileServerOptions = new FileServerOptions()
                {
                    EnableDirectoryBrowsing = commonConfig.EnableDirectoryBrowsing
                };
                fileServerOptions.StaticFileOptions.ServeUnknownFileTypes = commonConfig.ServeUnknownFileTypes;
                fileServerOptions.StaticFileOptions.DefaultContentType = "application/octet-stream";
                fileServerOptions.StaticFileOptions.RequestPath = commonConfig.StaticFilesRequestPath;
                fileServerOptions.StaticFileOptions.FileProvider = new PhysicalFileProvider(staticFileDir.FullName);

                fileServerOptions.StaticFileOptions.ContentTypeProvider = contentTypeProvider;
                application.UseFileServer(fileServerOptions);
#else
                application.UseStaticFiles(new StaticFileOptions
                {
                    ServeUnknownFileTypes = commonConfig.ServeUnknownFileTypes,
                    DefaultContentType = "application/octet-stream",
                    ContentTypeProvider = contentTypeProvider,
                    RequestPath = commonConfig.StaticFilesRequestPath,
                    FileProvider = new PhysicalFileProvider(staticFileDir.FullName),
                });
#endif
            }
        }

        /// <summary>
        /// Adds authentication service
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddWebapiAuthentication(this IServiceCollection services, AppSettings appSettings)
        {

            var dictionary = new ManegedTicketStoreDictionary();
            var authConfig = appSettings.Get<AuthenticationConfig>();
            var cacheConfig = appSettings.Get<DistributedCacheConfig>();
            if (authConfig.Any())
            {
                foreach (var scheme in authConfig)
                {
                    var authenticationBuilder = services.AddAuthentication(scheme.Name);
                    //add main cookie authentication
                    authenticationBuilder.AddCookie(scheme.Name, (serviceProvider, options) =>
                    {
                        options.Cookie.Name = $"{scheme.Name}_auth";
                        options.Cookie.SameSite = SameSiteMode.None;
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.SlidingExpiration = scheme.SlidingExpiration;
                        options.Events.OnRedirectToLogin = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        };
                        options.Events.OnRedirectToAccessDenied = context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        };
                        if (cacheConfig.Enabled && cacheConfig.DistributedCacheType == DistributedCacheType.Redis)
                        {
                            var sessionStore = new RedisCookieTicketStore(serviceProvider.GetRequiredService<IStaticCacheManager>(), scheme, options);
                            dictionary.Add(scheme.Name, sessionStore);
                            options.SessionStore = sessionStore;
                        }
                        else
                        {
                            var sessionStore = new FileCookieTicketStore(new DirectoryInfo(Path.Combine(NopConfigurationDefaults.AppSettingsDirectory.FullName, "Cookies")), scheme, options);
                            dictionary.Add(scheme.Name, sessionStore);
                            options.SessionStore = sessionStore;
                        }
                    });
                }
            }
            services.AddSingleton<IManegedTicketStoreDictionary>(dictionary);
        }

        public static AuthenticationBuilder AddCookie(this AuthenticationBuilder builder, string authenticationScheme, Action<IServiceProvider, CookieAuthenticationOptions> configureOptions)
        {
            builder.Services.AddOptions();
            builder.Services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>>(serviceProvider => new ConfigureNamedOptions<CookieAuthenticationOptions>(authenticationScheme, options => configureOptions(serviceProvider, options)));
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureCookieAuthenticationOptions>());
            return builder.AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>(authenticationScheme, null);
        }

        /// <summary>
        /// Add and configure default HTTP clients
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddWebApiHttpClients(this IServiceCollection services)
        {
            //default client
            services.AddHttpClient(NopHttpDefaults.DefaultHttpClient);
            services.AddHttpClient<KeepAliveHttpClient>(NopHttpDefaults.DefaultHttpClient, HttpClientConfigure);
            services.AddHttpClient<InternalHttpClient>(NopHttpDefaults.DefaultHttpClient, HttpClientConfigure);
        }

        static void HttpClientConfigure(IServiceProvider serviceProvider, HttpClient client)
        {
            var appSettings = serviceProvider.GetRequiredService<AppSettings>();
            var commonConfig = appSettings.Get<CommonConfig>();
            //configure client
            client.BaseAddress = new Uri(commonConfig.DefaultSiteUrl);
            client.DefaultRequestHeaders.ExpectContinue = false;
        }

        public static void AddWebSockets(this IServiceCollection services, AppSettings appSettings)
        {
            var config = appSettings.Get<WebSocketsConfig>();
            if (config.UseWebSockets)
            {
                services.AddSingleton<IUserWebsockets, UserWebSockets>();
                services.AddScoped<IWebSocketHandler, EchoWebSocketHandler>();
            }
        }
    }
}
