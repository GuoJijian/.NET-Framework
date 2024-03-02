using Webapi.Core.Domain;

namespace Webapi.Core.Configuration {
    public interface ISettingEntity : IBaseEntity {
        string Name { get; set; }
        string Value { get; set; }
    }
}