using Microsoft.EntityFrameworkCore;

using DAL.Core.Repository;
using DAL.Core.Attribute;

namespace DAL.Core
{
    [RepositoryTypes(typeof(IRepository<>), typeof(Repository<,>))]
    public class AttendanceServiceUtility : BaseDbContext
    {
        public AttendanceServiceUtility(DbContextOptions<AttendanceServiceUtility> options) : base(options)
        {
        }
    }
}
