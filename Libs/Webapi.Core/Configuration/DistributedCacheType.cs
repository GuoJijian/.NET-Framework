using System.Runtime.Serialization;

namespace Webapi.Core.Configuration
{
    public enum DistributedCacheType
    {
        [EnumMember(Value = "memory")]
        Memory,
        //[EnumMember(Value = "sqlserver")]
        //SqlServer,
        [EnumMember(Value = "redis")]
        Redis
    }
}
