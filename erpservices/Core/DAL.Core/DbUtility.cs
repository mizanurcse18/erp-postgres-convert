using Microsoft.EntityFrameworkCore;

using DAL.Core.Repository;
using DAL.Core.Attribute;

namespace DAL.Core
{
    [RepositoryTypes(typeof(IRepository<>), typeof(Repository<,>))]
    public class DbUtility : BaseDbContext
    {
        public DbUtility(DbContextOptions<DbUtility> options) : base(options)
        {
        }
    }
}
