using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Manager.Implementations
{

    public class BankManager : ManagerBase, IBankManager
    {
        private readonly IRepository<Bank> BankRepo;
        private readonly IRepository<GeneralLedger> GeneralLedgerRepo;
        public BankManager(IRepository<Bank> bankRepo, IRepository<GeneralLedger> generalLedgerRepo)
        {
            BankRepo = bankRepo;
            GeneralLedgerRepo = generalLedgerRepo;
        }

        public async Task<List<BankDto>> GetBankList()
        {
            //string sql = $@"select BankID, BankName, BankAddress, ConcernPersonName, ConcernPersonPhoneNumber from Bank";
            string sql = $@"select BN.BankID, BankName, BankAddress, ConcernPersonName, ConcernPersonPhoneNumber, 
                            (case when ck.BankID> 0 Then 0 else 1 end) as IsRemovable, BN.IsActive from Bank BN
                            LEFT JOIN ChequeBook ck on ck.BankID = BN.BankID";
            return await Task.FromResult(BankRepo.GetDataModelCollection<BankDto>(sql));
        }

        public void SaveChanges(BankDto bankDto)
        {
            EmailCalucation();
            using var unitOfWork = new UnitOfWork();
            var existBank = BankRepo.Entities.SingleOrDefault(x => x.BankID == bankDto.BankID).MapTo<Bank>();

            if (existBank.IsNull() || existBank.BankID.IsZero() || existBank.IsAdded)
            {
                bankDto.SetAdded();
                SetNewBankID(bankDto);
            }
            else
            {
                bankDto.SetModified();
            }
            var userEnt = bankDto.MapTo<Bank>();
            userEnt.CompanyID = bankDto.CompanyID ?? AppContexts.User.CompanyID;


            BankRepo.Add(userEnt);
            unitOfWork.CommitChangesWithAudit();
        }

        private void SetNewBankID(BankDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Bank", AppContexts.User.CompanyID);
            obj.BankID = code.MaxNumber;
        }

        public async Task<BankDto> GetBank(int bankId)
        {
            var bank = BankRepo.Entities.SingleOrDefault(x => x.BankID == bankId).MapTo<BankDto>();
            return await Task.FromResult(bank);
        }

        public bool DeleteBank(int bankId)
        {
            var sql = @$"select count(*) as Count from ChequeBook where BankID = {bankId}";
            var exist = BankRepo.GetData(sql);
            int canDelete = Convert.ToInt32(exist["Count"]);
            if (canDelete > 0) { return false; }

            using var unitOfWork = new UnitOfWork();
            var bank = BankRepo.Entities.SingleOrDefault(x => x.BankID == bankId);
            bank.SetDeleted();
            BankRepo.Add(bank);

            unitOfWork.CommitChangesWithAudit();
            return true;                         
        }

    }
}
