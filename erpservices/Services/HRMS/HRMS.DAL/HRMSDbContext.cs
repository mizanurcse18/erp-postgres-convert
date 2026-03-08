using DAL.Core;
using DAL.Core.Attribute;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Security.DAL.Entities;

namespace HRMS.DAL
{
    [RepositoryTypes(typeof(IRepository<>), typeof(Repository<,>))]
    class HRMSDbContext : BaseDbContext
    {
        public HRMSDbContext(DbContextOptions<HRMSDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Designation> Designations { get; set; }
        public virtual DbSet<Division> Divisions { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<EmployeeSupervisorMap> EmployeeSupervisorMaps { get; set; }
        public virtual DbSet<Employment> Employments { get; set; }
        public virtual DbSet<BranchInfo> BranchInfos { get; set; }
        public virtual DbSet<RemoteAttendance> RemoteAttendances { get; set; }
        public virtual DbSet<Cluster> Clusters { get; set; }
        public virtual DbSet<Region> Reigions { get; set; }
        public virtual DbSet<ShiftingMaster> ShiftingMasters { get; set; }
        public virtual DbSet<ShiftingChild> ShiftingChilds { get; set; }
        public virtual DbSet<WorkingDay> WorkingDays { get; set; }
        public virtual DbSet<Holiday> Holidays { get; set; }
        public virtual DbSet<CompanyLeavePolicy> CompanyLeavePolicyList { get; set; }
        public virtual DbSet<EmployeeLeaveAccount> EmployeeLeaveAccounts { get; set; }
        public virtual DbSet<EmployeeLeaveApplication> EmployeeLeaveApplications { get; set; }
        public virtual DbSet<EmployeeLeaveApplicationDayBreakDown> EmployeeLeaveApplicationDayBreakDownList { get; set; }
        public virtual DbSet<WalletConfiguration> WalletConfigurationList { get; set; }
        public virtual DbSet<WageCodeConfiguration> WageCodeConfigurationList { get; set; }
        public virtual DbSet<LFADeclaration> LFADeclarations { get; set; }
        public virtual DbSet<AttendanceSummary> AttendanceSummaryList { get; set; }
        public virtual DbSet<LeavePolicySettings> LeavePolicySettings { get; set; }
        public virtual DbSet<JobGrade> JobGrades { get; set; }
        public virtual DbSet<EmployeeAccessDeactivation> EmployeeAccessDeactivationList { get; set; }
        public virtual DbSet<EmployeeExitInterview> EmployeeExitInterviewList { get; set; }
        public virtual DbSet<DivisionHeadMap> DivisionHeadMapList { get; set; }
        public virtual DbSet<DepartmentHeadMap> DepartmentHeadMapList { get; set; }
        public virtual DbSet<AnnualLeaveEncashmentWindowMaster> AnnualLeaveEncashmentWindowMasterList { get; set; }
        public virtual DbSet<AnnualLeaveEncashmentWindowChild> AnnualLeaveEncashmentWindowChildList { get; set; }
        public virtual DbSet<AnnualLeaveEncashmentPolicySettings> AnnualLeaveEncashmentPolicySettingsList { get; set; }
        public virtual DbSet<AnnualLeaveEncashmentMaster> AnnualLeaveEncashmentMasterList { get; set; }
        public virtual DbSet<DocumentUpload> DocumentUploadList { get; set; }
        public virtual DbSet<DocumentUploadResponse> DocumentUploadResponseList { get; set; }
        public virtual DbSet<RequestSupportMaster> RequestSupportMasterList { get; set; }
        public virtual DbSet<RequestSupportVehicleDetails> RequestSupportVehicleDetailsList { get; set; }
        public virtual DbSet<RequestSupportFacilitiesDetails> RequestSupportFacilitiesDetailsList { get; set; }
        public virtual DbSet<RequestSupportItemDetails> RequestSupportItemDetailsList { get; set; }
        public virtual DbSet<EmailNotification> EmailNotificationList { get; set; }
        public virtual DbSet<UnauthorizedLeaveEmailDate> UnauthorizedLeaveEmailDateList { get; set; }
        public virtual DbSet<RenovationORMaintenanceCategory> RenovationORMaintenanceCategoryList { get; set; }
        public virtual DbSet<RequestSupportRenovationORMaintenanceDetails> RequestSupportRenovationORMaintenanceDetailsList { get; set; }
        public virtual DbSet<ShiftingLeaveChild> ShiftingLeaveChildList { get; set; }
        public virtual DbSet<SupportRequisitionMaster> SupportRequisitionMasterList { get; set; }
        public virtual DbSet<AssetRequisitionCategoryChild> AssetRequisitionCategoryChildList { get; set; }
        public virtual DbSet<AccessoriesRequisitionCategoryChild> AccessoriesRequisitionCategoryChildList { get; set; }
        public virtual DbSet<AccessRequestCategoryChild> AccessRequestCategoryChildList { get; set; }
        public virtual DbSet<AuditQuestion> AuditQuestionList { get; set; }
        public virtual DbSet<AuditApprovalConfig> AuditApprovalConfigList { get; set; }
        public virtual DbSet<ExternalAuditMaster> ExternalAuditMasterList { get; set; }
        public virtual DbSet<ExternalAuditChild> ExternalAuditChildList { get; set; }
        public virtual DbSet<UserWiseUddoktaOrMerchantMapping> UserWiseUddoktaOrMerchantMapping { get; set; }   
        public virtual DbSet<ExternalAuditConfig> ExternalAuditConfiglist { get; set; }   
        public virtual DbSet<EmployeeBankInfo> EmployeeBankInfolist { get; set; }
        public virtual DbSet<PayrollAuditTrial> PayrollAuditTriallist { get; set; }
        public virtual DbSet<EmployeePaySlipInfo> EmployeePaySlipInfolist { get; set; }
        public virtual DbSet<EmployeeFestivalBonusInfo> EmployeeFestivalBonusInfolist { get; set; }
        public virtual DbSet<EmployeeMonthlyIncentiveInfo> EmployeeMonthlyIncentiveInfolist { get; set; }
        public virtual DbSet<EmployeeRegularIncentiveInfo> EmployeeRegularIncentiveInfolist { get; set; }
    }
}
