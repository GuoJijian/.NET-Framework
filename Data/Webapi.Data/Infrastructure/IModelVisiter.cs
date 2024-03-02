using Microsoft.EntityFrameworkCore;

namespace Webapi.Data.Infrastructure
{
    public interface IModelVisiter
    {
        void Visit(ModelBuilder modelBuilder);
    }
}
