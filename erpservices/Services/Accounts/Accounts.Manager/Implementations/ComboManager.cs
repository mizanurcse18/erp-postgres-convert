using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Core;
using Core.AppContexts;
using DAL.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Manager.Implementations
{
    public class ComboManager : IComboManager
    {
        private readonly IRepository<IOUMaster> IOUMasterRepo;
        private readonly IRepository<ExpenseClaimMaster> ExpenseClaimMasterRepo;
        private readonly IRepository<GeneralLedger> GeneralLedgerRepo;
        private readonly IRepository<Bank> BankRepo;
        private readonly IRepository<ChequeBook> ChequebookRepo;
        private readonly IRepository<ACCategory> CategoryRepo;
        private readonly IRepository<ChartOfAccounts> ChartOfAccountsRepo;
        private readonly IRepository<CustodianWallet> CustWalletRepo;

        public ComboManager(IRepository<IOUMaster> iouMasterRepo, IRepository<ExpenseClaimMaster> expenseClaimMasterRepo, IRepository<GeneralLedger> generalLedgerRepo, IRepository<Bank> bankRepo, IRepository<ChequeBook> chequebookRepo,
            IRepository<ACCategory> categoryRepo, IRepository<ChartOfAccounts> chartOfAccountsRepo, IRepository<CustodianWallet> custWalletRepo)
        {
            IOUMasterRepo = iouMasterRepo;
            ExpenseClaimMasterRepo = expenseClaimMasterRepo;
            GeneralLedgerRepo = generalLedgerRepo;
            BankRepo = bankRepo;
            ChequebookRepo = chequebookRepo;
            CategoryRepo = categoryRepo;
            ChartOfAccountsRepo = chartOfAccountsRepo;
            CustWalletRepo = custWalletRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetGLCombo(string param)
        {
            string sql = @$"SELECT GLID AS value, GLName AS label  FROM GeneralLedger
                            WHERE GLName Like '%{param}%' GROUP BY GLID, GLName";
            var listDict = GeneralLedgerRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }
        public async Task<List<ComboModel>> GetIOUList()
        {
            string sql = @$"SELECT IOU.IOUMasterID AS value
	                ,IOU.ReferenceNo AS label
	                ,ECM.ECMasterID
                FROM IOUMaster IOU
                LEFT JOIN ExpenseClaimMaster ECM ON ECM.IOUMasterID = IOU.IOUMasterID
                LEFT JOIN IOUOrExpensePaymentChild PC ON PC.IOUOrExpenseClaimID = IOU.IOUMasterID
                LEFT JOIN IOUOrExpensePaymentMaster PM on PM.PaymentMasterID=PC.PaymentMasterID
                WHERE isnull(PM.IsSettlement,0)=1 AND IOU.EmployeeID = {AppContexts.User.EmployeeID}
                	AND IOU.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}
                	AND (
                		ECM.ECMasterID IS NULL
                		OR (ECM.ApprovalStatusID = {(int)Util.ApprovalStatus.Rejected})
                		)";

            return IOUMasterRepo.GetDataModelCollection<ComboModel>(sql);

        }

        public async Task<List<ComboModel>> GetGLComboList()
        {
            //string sql = @$"SELECT GLID AS value, GLName AS label from GeneralLedger";
            //string sql = @$"SELECT GLID AS GL, GLName AS 'GL Head', GLID AS Bank, GLName AS 'Bank Name' from GeneralLedger";
            //var listDict = GeneralLedgerRepo.GetDataDictCollection(sql);
            //return await Task.FromResult(listDict);

            var allCOAListGlList = ChartOfAccountsRepo.GetAllListAsync().Result.Where(coa => coa.CategoryID == Convert.ToInt32(Util.COAAccountCategory.Ledger) && coa.IsActive == true).ToList();
            return allCOAListGlList.Select(x => new ComboModel { value = Convert.ToInt32(x.COAID), label = x.AccountName }).ToList();
        }

        public async Task<List<ComboModel>> GetWalletList()
        {
            var allActiveWallet = CustWalletRepo.GetAllListAsync().Result.Where(w=> w.IsActive == true).ToList();
            return allActiveWallet.Select(x => new ComboModel { value = Convert.ToInt32(x.CWID), label = x.CWID+ "-" + x.WalletName }).ToList();
        }

        public async Task<List<BankDto>> GetBankList()
        {
            string sql = $@"select BankID, BankName, BankAddress, ConcernPersonName, ConcernPersonPhoneNumber from Bank where IsActive = 1";
            return BankRepo.GetDataModelCollection<BankDto>(sql);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetAllChequebookForDropdown()
        {
            string sql = @$"select c.CBID ChequeBookID,c.BankID,c.ChequeBookNo,c.AccountNo,c.BranchName,c.RoutingName,c.SwiftCode,c.NoOfPage
                                    ,b.BankName VendorBankName,CONCAT(b.BankName,'-',c.BranchName,'-',c.AccountNo) Details 
                            from 
                            ChequeBook c
                            LEFT JOIN Bank b ON c.BankID=b.BankID
                            where c.IsActive=1";
            var listDict = ChequebookRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<List<ComboModel>> GetCOACategoryListCombo()
        {
            var approvalpanelList = await CategoryRepo.GetAllListAsync();
            return approvalpanelList.Select(x => new ComboModel { value = x.CategoryID, label = x.CategoryName }).ToList();
        }

        public async Task<List<ComboModel>> GetCOAGLListCombo(string param)
        {
            if (string.IsNullOrEmpty(param) || param.Equals("0"))
            {
                var allCOAListGlList = ChartOfAccountsRepo.GetAllListAsync().Result.Where(coa => coa.CategoryID == Convert.ToInt32(Util.COAAccountCategory.Ledger) && coa.IsActive == true).ToList();
                return allCOAListGlList.Select(x => new ComboModel { value = Convert.ToInt32(x.COAID), label = x.AccountName + "|" + x.AccountCode }).ToList();

            }

            var allCOAListGlFilterList = ChartOfAccountsRepo.GetAllListAsync().Result.Where(coa => coa.CategoryID == Convert.ToInt32(Util.COAAccountCategory.Ledger) && coa.IsActive == true).ToList();
            return allCOAListGlFilterList.Where(coa=> coa.AccountName.ToLower().Contains(param.ToLower())).Select(x => new ComboModel { value = Convert.ToInt32(x.COAID), label = x.AccountName + "|" + x.AccountCode }).ToList();
        }

        public async Task<List<ComboModel>> GetCOAChqBookCombo(string param)
        {
            string sql = @$"select CBID value, ChequeBookNo label from ChequeBook
                            WHERE IsActive = 1
                            AND GLID = {param}";
            var chqBooksCombo = ChequebookRepo.GetDataModelCollection<ComboModel>(sql);
            return chqBooksCombo;
        }

        public async Task<List<ComboModel>> GetCOAChqBookPageCombo(string param)
        {
            string sql = @$"select CBCID value, LeafNo label  from ChequeBookChild
                            where CBID = {param} AND IsActiveLeaf = 1 AND IsUsed = 0 order by CBCID";
            var chqBooksCombo = ChequebookRepo.GetDataModelCollection<ComboModel>(sql);
            return chqBooksCombo;
        }
    }
}
