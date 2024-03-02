using Autofac;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using System;
using Webapi.Core;
using Webapi.Core.Caching;
using Webapi.Core.Configuration;
using Webapi.Data.Caching;

namespace Webapi.Server
{
    public class ServerDependencyRegistrar : IDependencyRegistrar<AppSettings>
    {
        public int Order => 2000;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, AppSettings appSettings)
        {
            var distributedCacheConfig = appSettings.Get<DistributedCacheConfig>();
            if (distributedCacheConfig.Enabled)
            {
                switch (distributedCacheConfig.DistributedCacheType)
                {
                    case DistributedCacheType.Redis:
                        {
                            //缓存选项
                            var option = new RedisCacheOptions
                            {
                                Configuration = distributedCacheConfig.ConnectionString
                            };
                            builder.RegisterInstance(Options.Create(option).Value).As<IOptions<RedisCacheOptions>>().SingleInstance();
                            //缓存实现类
                            builder.RegisterType<RedisDistributedCache>().As<IRedisDistributedCache>().SingleInstance();
                            //缓存操作类
                            builder.RegisterType<DistributedCacheManager>().As<IStaticCacheManager>().As<ILocker>().SingleInstance();
                        }
                        break;

                    //case DistributedCacheType.SqlServer:
                    //    {
                    //        //缓存选项
                    //        var option = new SqlServerCacheOptions
                    //        {
                    //            ConnectionString = distributedCacheConfig.ConnectionString,
                    //            TableName = distributedCacheConfig.TableName,
                    //            SchemaName = distributedCacheConfig.SchemaName
                    //        };
                    //        builder.RegisterInstance(Options.Create(option).Value).As<IOptions<SqlServerCacheOptions>>().SingleInstance();
                    //        //缓存实现类
                    //        builder.RegisterType<SqlServerCache>().As<IDistributedCache>().InstancePerLifetimeScope();
                    //        //缓存操作类
                    //        builder.RegisterType<DistributedCacheManager>().As<IStaticCacheManager>().As<ILocker>().InstancePerLifetimeScope();
                    //    }
                    //    break;

                    case DistributedCacheType.Memory:
                        {
                            //缓存选项
                            builder.RegisterType<MemoryCacheOptions>().As<IOptions<MemoryCacheOptions>>().SingleInstance();
                            //缓存实现类
                            builder.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();
                            //缓存操作类
                            builder.RegisterType<MemoryCacheManager>().As<IStaticCacheManager>().As<ILocker>().SingleInstance();
                        }
                        break;

                    default:
                        throw new NotSupportedException($"不支持的分布式缓存选项：{distributedCacheConfig.DistributedCacheType}！");

                }
            }
            else
            {
                //注册空缓存类型，无缓存操作，只是保持 IStaticCacheManager 对象可用
                builder.RegisterType<EmptyCacheManager>().As<IStaticCacheManager>().As<ILocker>().SingleInstance();
            }
        }
    }
}
