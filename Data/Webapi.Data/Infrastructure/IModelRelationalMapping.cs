using Microsoft.EntityFrameworkCore;

namespace Webapi.Data.Infrastructure
{
    public interface IModelRelationalMapping
    {
        void OnMapping(ModelBuilder modelBuilder, string tablenameprefix);
    }
}
