using Approval.DAL.Entities;
using DAL.Core;
using DAL.Core.Attribute;
using DAL.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace Approval.DAL
{
    [RepositoryTypes(typeof(IRepository<>), typeof(Repository<,>))]
    class ApprovalDbContext : BaseDbContext
    {
        public ApprovalDbContext(DbContextOptions<ApprovalDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public virtual DbSet<ApprovalType> ApprovalTypeList { get; set; }
        public virtual DbSet<ApprovalProcessPanelMap> ApprovalProcessPanelMapList { get; set; }
        public virtual DbSet<ApprovalProcess> ApprovalProcessList { get; set; }
        public virtual DbSet<ApprovalPanelEmployee> ApprovalPanelEmployeeList { get; set; }
        public virtual DbSet<ApprovalPanel> ApprovalPanelList { get; set; }
        public virtual DbSet<ApprovalForwardInfo> ApprovalForwardInfoList { get; set; }
        public virtual DbSet<ApprovalFeedback> ApprovalFeedbackList { get; set; }
        public virtual DbSet<ApprovalEmployeeFeedbackRemarks> ApprovalEmployeeFeedbackRemarksList { get; set; }
        public virtual DbSet<ApprovalEmployeeFeedback> ApprovalEmployeeFeedbackList { get; set; }
        public virtual DbSet<APStatus> APStatusList { get; set; }
        public virtual DbSet<ApprovalPanelForwardEmployee> ApprovalPanelForwardEmployeeList { get; set; }
        public virtual DbSet<ApprovalPanelProxyEmployee> ApprovalPanelProxyEmployeeList { get; set; }
        public virtual DbSet<ManualApprovalPanelEmployee> ManualApprovalPanelEmployeeList { get; set; }
        public virtual DbSet<ApprovalMultiProxyEmployeeInfo> ApprovalMultiProxyEmployeeInfoList { get; set; }
        public virtual DbSet<DocumentApprovalMaster> DocumentApprovalMasterList { get; set; }
        public virtual DbSet<ManualApprovalPanelProxyEmployee> ManualApprovalPanelProxyEmployeeList { get; set; }
        public virtual DbSet<DocumentApprovalTemplate> DocumentApprovalTemplateList { get; set; }
        public virtual DbSet<ApprovalPanelEmployeeConfig> ApprovalPanelEmployeeConfigList { get; set; }
        public virtual DbSet<ApprovalPanelProxyEmployeeConfig> ApprovalPanelProxyEmployeeConfigList { get; set; }
        public virtual DbSet<DOAApprovalPanelEmployee> DOAApprovalPanelEmployeeList { get; set; }
        public virtual DbSet<DOAMaster> DOAMasterList { get; set; }
        public virtual DbSet<DynamicApprovalPanelEmployee> DynamicApprovalPanelEmployeeList { get; set; }


    }
}
