using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.DAL.Entities;
using HRMS.Manager.Interfaces;
using Core;

namespace HRMS.Manager.Implementations
{

    public class UddoktaMerchantTagUntagManager : ManagerBase, IUddoktaMerchantTagUntagManager
    {
        private readonly IRepository<UserWiseUddoktaOrMerchantMapping> UserWiseUddoktaOrMerchantRepo;
        private readonly IRepository<ExternalAuditMaster> MasterRepo;
        private readonly IRepository<DAL.Entities.ExternalAuditConfig> AuditConfig;

        public UddoktaMerchantTagUntagManager(IRepository<UserWiseUddoktaOrMerchantMapping> userWiseUddoktaOrMerchantRepo,  IRepository<ExternalAuditMaster> masterRepo,
            IRepository<DAL.Entities.ExternalAuditConfig> auditConfig
            )
        {
            UserWiseUddoktaOrMerchantRepo = userWiseUddoktaOrMerchantRepo;
            MasterRepo = masterRepo;
            AuditConfig = auditConfig;
        }

        public async Task<List<Dictionary<string, object>>> GetUddoktaMerchantList()
        {
            string sql = $@"select
                            ROW_NUMBER() over (order by m.EmployeeID desc) as SL
                            ,MAPID
                            ,e.EmployeeCode
                            ,m.EmployeeID
                            ,e.FullName
                            ,m.WalletNumber
                            ,WalletName
                            ,TypeID
                            ,s.SystemVariableCode TypeName
                            ,m.CompanyID
                            ,m.IsActive
                            ,IsTagged

                            from UserWiseUddoktaOrMerchantMapping m
                            left join Employee e on e.EmployeeID = m.EmployeeID
                            left join {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable s on s.SystemVariableID = m.TypeID";

            var result = UserWiseUddoktaOrMerchantRepo.GetDataDictCollection(sql);

            return await Task.FromResult(result.ToList());
        }

        public async Task<(bool, string)> SaveChanges(UserWiseUddoktaOrMerchantMapping walletModel)
        {
            var auditConfig = AuditConfig.Entities.FirstOrDefault(c => c.IsActive == true);

            var mappedUddoktaWallet = UserWiseUddoktaOrMerchantRepo.Entities.Where(u => u.EmployeeID == walletModel.EmployeeID && u.TypeID == 235 && u.IsTagged == true && u.IsActive == true);
            var mappedMerchantWallet = UserWiseUddoktaOrMerchantRepo.Entities.Where(u => u.EmployeeID == walletModel.EmployeeID && u.TypeID == 236 && u.IsTagged == true && u.IsActive == true);
            var taggedWallet = UserWiseUddoktaOrMerchantRepo.FirstOrDefault(u => u.WalletNumber == walletModel.WalletNumber && u.IsTagged == true &&u.IsActive == true);
            var IsTaggedWithUser = taggedWallet!=null ?  taggedWallet.EmployeeID == walletModel.EmployeeID ? true : false: false;
            var pendingAuditList = MasterRepo.Entities.Where(d=>d.MercentOrUdoktaID == walletModel.MAPID && (d.ApprovalStatusID == (int)Util.ApprovalStatus.Pending || d.ApprovalStatusID == (int)Util.ApprovalStatus.Initiated)).ToList();
            if (pendingAuditList.Count > 0)
            {
                return (false, "There is a pending audit with this wallet.");
            }

            if ((taggedWallet != null && IsTaggedWithUser == false) || IsTaggedWithUser == true && walletModel.IsTagged == true)
            {
                return (false, "Wallet is already tagged");
            }
            if ((mappedUddoktaWallet.Count() + mappedMerchantWallet.Count()) == (auditConfig.NumberOfUddokta + auditConfig.NumberOfMerchant) && IsTaggedWithUser == false && walletModel.IsTagged == true)
            {
                return (false, "Wallet add limit exceed");
            }

            if (mappedMerchantWallet.Count() == auditConfig.NumberOfMerchant && walletModel.TypeID == (int)Util.ExternalAuditWalletType.MERCHANT && IsTaggedWithUser == false && walletModel.IsTagged == true)
            {
                return (false, "Merchant tagging maximum limit exceed");
            }

            if (mappedUddoktaWallet.Count() == auditConfig.NumberOfUddokta && walletModel.TypeID == (int)Util.ExternalAuditWalletType.UDDOKTA && IsTaggedWithUser == false && walletModel.IsTagged == true)
            {
                return (false, "Uddokta tagging maximum limit exceed");
            }

            string respMsg = string.Empty;
         
            var existData = UserWiseUddoktaOrMerchantRepo.Entities.SingleOrDefault(x => x.MAPID == walletModel.MAPID).MapTo<UserWiseUddoktaOrMerchantMapping>();
            if (existData.IsNull() || existData.MAPID.IsZero() || existData.IsAdded)
            {
                walletModel.IsActive = true;
                walletModel.SetAdded();
                respMsg = "Wallet successfully tagged";
            }
            else
            {
                walletModel.MAPID = existData.MAPID;
                walletModel.WalletName = walletModel.IsTagged == false ?  existData.WalletName : walletModel.WalletName;
                walletModel.CreatedBy = existData.CreatedBy;
                walletModel.CreatedDate = existData.CreatedDate;
                walletModel.CreatedIP = existData.CreatedIP;
                walletModel.RowVersion = existData.RowVersion;
                walletModel.TypeID = existData.TypeID;
                walletModel.IsActive = true;
                walletModel.SetModified();

                respMsg = "Wallet successfully updated";
            }

            SetAuditFields(walletModel);

            using (var unitOfWork = new UnitOfWork())
            {
                UserWiseUddoktaOrMerchantRepo.Add(walletModel);
                unitOfWork.CommitChangesWithAudit();
            }

            return (true, respMsg);
        }


        public async Task<Dictionary<string, object>> GetUddoktaMerchant(int MapID)
        {
            string sql = $@"select
                            MAPID
                            ,e.EmployeeCode
                            ,m.EmployeeID
                            ,e.FullName
                            ,m.WalletNumber
                            ,WalletName
                            ,TypeID
                            ,s.SystemVariableCode TypeName
                            ,m.CompanyID
                            ,m.IsActive
                            ,IsTagged

                            from UserWiseUddoktaOrMerchantMapping m
                            left join Employee e on e.EmployeeID = m.EmployeeID
                            left join {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable s on s.SystemVariableID = m.TypeID
                            WHERE MAPID = {MapID}";

            var result = UserWiseUddoktaOrMerchantRepo.GetDataDictCollection(sql);

            return await Task.FromResult(result.FirstOrDefault());
        }

        public bool Delete(int MapID)
        {
            var sql = @$"select count(*) as Count from ChequeBook where BankID = {MapID}";
            var exist = UserWiseUddoktaOrMerchantRepo.GetData(sql);
            int canDelete = Convert.ToInt32(exist["Count"]);
            if (canDelete > 0) { return false; }

            using var unitOfWork = new UnitOfWork();
            var umMap = UserWiseUddoktaOrMerchantRepo.Entities.SingleOrDefault(x => x.MAPID == MapID);
            umMap.SetDeleted();
            UserWiseUddoktaOrMerchantRepo.Add(umMap);

            unitOfWork.CommitChangesWithAudit();
            return true;                         
        }

    }
}
