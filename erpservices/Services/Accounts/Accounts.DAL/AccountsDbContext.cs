using Accounts.DAL.Entities;
using DAL.Core;
using DAL.Core.Attribute;
using DAL.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace Accounts.DAL
{
    [RepositoryTypes(typeof(IRepository<>), typeof(Repository<,>))]
    class AccountsDbContext: BaseDbContext
    {
        public AccountsDbContext(DbContextOptions<AccountsDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            base.OnModelCreating(modelBuilder);
        }
        public virtual DbSet<IOUMaster> IOUMasters { get; set; }
        public virtual DbSet<IOUChild> IOUChilds { get; set; }
        public virtual DbSet<ExpenseClaimMaster> ExpenseClaimMasters { get; set; }
        public virtual DbSet<ExpenseClaimChild> ExpenseClaimChilds { get; set; }
        public virtual DbSet<GeneralLedger> GeneralLedgers { get; set; }
        public virtual DbSet<BudgetMaster> BudgetMasters { get; set; }
        public virtual DbSet<BudgetChild> BudgetChilds { get; set; }
        public virtual DbSet<IOUOrExpensePaymentMaster> IOUOrExpensePaymentMasters { get; set; }
        public virtual DbSet<IOUOrExpensePaymentChild> IOUOrExpensePaymentChilds { get; set; }
        public virtual DbSet<BudgetChildWithApprovalPanelMap> BudgetChildWithApprovalPanelMaps { get; set; }
        public virtual DbSet<CostCategory> CostCategorys { get; set; }
        public virtual DbSet<CostCenter> CostCenters { get; set; }
        public virtual DbSet<TaxationVettingMaster> TaxationVettingMasterList { get; set; }
        public virtual DbSet<VatTaxDeductionSource> VatTaxDeductionSourceList { get; set; }
        public virtual DbSet<TaxationVettingPayment> TaxationVettingPaymentList { get; set; }
        public virtual DbSet<ChequeBook> ChequeBooks { get; set; }
        public virtual DbSet<Bank> Banks { get; set; }
        public virtual DbSet<TaxationVettingPaymentChild> TaxationVettingPaymentChild { get; set; }
        public virtual DbSet<TaxationVettingPaymentMethod> TaxationVettingPaymentMethod { get; set; }
        public virtual DbSet<ChequeBookChild> ChequeBookChildList { get; set; }
        public virtual DbSet<ChartOfAccounts> ChartOfAccountsList { get; set; }
        public virtual DbSet<ACClass> ACClassList { get; set; }
        public virtual DbSet<ACCategory> ACCategoryList { get; set; }
        public virtual DbSet<CustodianWallet> CustodianWalletList { get; set; }
        public virtual DbSet<PettyCashAdvanceMaster> PettyCashAdvanceMasterList { get; set; }
        public virtual DbSet<PettyCashAdvanceChild> PettyCashAdvanceChildList { get; set; }
        public virtual DbSet<PettyCashExpenseMaster> PettyCashExpenseMasterList { get; set; }
        public virtual DbSet<PettyCashExpenseChild> PettyCashExpenseChildList { get; set; }
        public virtual DbSet<PettyCashReimburseMaster> PettyCashReimburseMasterList { get; set; }
        public virtual DbSet<PettyCashReimburseChild> PettyCashReimburseChildList { get; set; }
        public virtual DbSet<PettyCashPaymentMaster> PettyCashPaymentMasterList { get; set; }
        public virtual DbSet<PettyCashPaymentChild> PettyCashPaymentChildList { get; set; }
        public virtual DbSet<PettyCashTransactionHistory> PettyCashTransactionHistoryList { get; set; }
        public virtual DbSet<VoucherCategory> VoucherCategory { get; set; }
        public virtual DbSet<VoucherMaster> VoucherMaster { get; set; }
        public virtual DbSet<VoucherChild> VoucherChild { get; set; }

    }
}
