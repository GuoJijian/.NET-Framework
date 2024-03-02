using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Webapi.Core.Domain;
using Webapi.Core.Domain.Logs;

namespace Webapi.Data.Mapping
{
    public class AdminLogMap : UserLogBaseMap<Admin, AdminLog>
    {
    }
}
