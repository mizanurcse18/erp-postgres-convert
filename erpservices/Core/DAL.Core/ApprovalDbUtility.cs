using Microsoft.EntityFrameworkCore;

using DAL.Core.Repository;
using DAL.Core.Attribute;

namespace DAL.Core
{
    [RepositoryTypes(typeof(IRepository<>), typeof(Repository<,>))]
    public class ApprovalDbUtility : BaseDbContext
    {
        public ApprovalDbUtility(DbContextOptions<ApprovalDbUtility> options) : base(options)
        {
        }
    }
}
