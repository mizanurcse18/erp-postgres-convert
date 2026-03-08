using DAL.Core;
using DAL.Core.Attribute;
using DAL.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace Security.DAL
{
    [RepositoryTypes(typeof(IRepository<>), typeof(Repository<,>))]
    class SecurityDbContext : BaseDbContext
    {
        public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(b => b.UserName);
            modelBuilder.Entity<UserCompany>()
                .HasKey(mp => new { mp.UserID, mp.CompanyID });
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserCompany> UserCompanies { get; set; }
        public virtual DbSet<UserLogTracker> UserLogTrackers { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<SecurityGroupMaster> SecurityGroupMasters { get; set; }
        public virtual DbSet<SecurityGroupRuleChild> SecurityGroupRules { get; set; }
        public virtual DbSet<SecurityGroupUserChild> SecurityGroupUsers { get; set; }
        public virtual DbSet<SecurityRuleMaster> SecurityRuleMasters { get; set; }
        public virtual DbSet<SecurityRulePermissionChild> SecurityRulePermissions { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<PersonAcademicInfo> PersonAcademicInfos { get; set; }
        public virtual DbSet<PersonAddressInfo> AddressInfos { get; set; }
        public virtual DbSet<PersonEmploymentInfo> PersonEmploymentInfos { get; set; }
        public virtual DbSet<PersonFamilyInfo> PersonFamilyInfos { get; set; }
        public virtual DbSet<PersonImage> PersonImages { get; set; }
        public virtual DbSet<PersonProfessionalCertificationInfo> PersonProfessionalCertificationInfos { get; set; }
        public virtual DbSet<PersonTrainingInfo> PersonTrainingInfos { get; set; }
        public virtual DbSet<SystemVariable> SystemVariables { get; set; }
        public virtual DbSet<BankAccountInfo> BankAccountInfos { get; set; }
        public virtual DbSet<District> Districts { get; set; }
        public virtual DbSet<Division> Divisions { get; set; }
        public virtual DbSet<Thana> Tahans { get; set; }
        public virtual DbSet<IPAddress> IPAddressList { get; set; }
        public virtual DbSet<PersonWorkExperience> PersonWorkExperiences { get; set; }
        public virtual DbSet<PersonAwardInfo> PersonAwardInfos { get; set; }
        public virtual DbSet<PersonReferenceInfo> PersonReferenceInfos { get; set; }
        public virtual DbSet<PersonEmergencyContactInfo> PersonEmergencyContactInfos { get; set; }
        public virtual DbSet<FinancialYear> FinancialYears { get; set; }
        public virtual DbSet<Period> Periods { get; set; }
        public virtual DbSet<UserThemeSetting> UserThemeSettingList { get; set; }
        public virtual DbSet<FileUpload> FileUploads { get; set; }
        public virtual DbSet<OnboardingUser> OnboardingUserList { get; set; }
        public virtual DbSet<NomineeInformation> NomineeInformationList { get; set; }
        public virtual DbSet<NFAMaster> NFAMasters { get; set; }
        public virtual DbSet<NFAChild> NFAChilds { get; set; }
        public virtual DbSet<ReportSuite> ReportSuites { get; set; }
        public virtual DbSet<ReportSuiteField> ReportSuiteFields { get; set; }
        public virtual DbSet<ReportSuiteParentField> ReportSuiteParentFields { get; set; }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<TutorialMaster> TutorialMasters { get; set; }
        public virtual DbSet<CompanyTerms> CompanyTermsList { get; set; }
        public virtual DbSet<Currency> CurrencyList { get; set; }
        public virtual DbSet<EmployeeProfileApproval> EmployeeProfileApprovalList { get; set; }
        public virtual DbSet<MenuApiPaths> MenuApiPathsList { get; set; }
        public virtual DbSet<UsersOTP> UsersOTPList { get; set; }
        public virtual DbSet<AssessmentYear> AssessmentYearList { get; set; }
        public virtual DbSet<NFAChildStrategic> NFAChildStrategicList { get; set; }
        public virtual DbSet<Location> LocationList { get; set; }
        public virtual DbSet<VehicleDetails> VehicleDetailsList { get; set; }
        public virtual DbSet<DriverDetails> DriverDetailsList { get; set; }
        public virtual DbSet<SystemConfiguration> SystemConfigurationList { get; set; }
        public virtual DbSet<UserTokenBlackList> UsersTokenBlackList { get; set; }
        public virtual DbSet<CommonInterface> CommonInterfaceList { get; set; }
        public virtual DbSet<CommonInterfaceFields> CommonInterfaceFieldsList { get; set; }
        public virtual DbSet<BusinessSupportItem> BusinessSupportItems { get; set; }
        public virtual DbSet<SupportRequisitionItem> SupportRequisitionItems { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }

    }
}
