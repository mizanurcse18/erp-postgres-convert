using DAL.Core;
using DAL.Core.Attribute;
using DAL.Core.Repository;
using Mail.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mail.DAL
{
    [RepositoryTypes(typeof(IRepository<>), typeof(Repository<,>))]
    class MailDbContext : BaseDbContext
    {
        public MailDbContext(DbContextOptions<MailDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<MailConfiguration> MailConfigurationList { get; set; }
        public virtual DbSet<MailGroupSetup> MailGroupSetupList { get; set; }
        public virtual DbSet<MailSetup> MailSetupList { get; set; }
    }
}
