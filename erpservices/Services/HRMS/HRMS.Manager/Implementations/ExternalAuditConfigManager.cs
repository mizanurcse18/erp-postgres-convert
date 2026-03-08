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

    public class ExternalAuditConfigManager : ManagerBase, IExternalAuditConfigManager
    {
        private readonly IRepository<ExternalAuditConfig> ExternalAuditConfigRepo;
        private readonly IRepository<ExternalAuditMaster> ExternalAuditMasterRepo;
        private readonly IRepository<UserWiseUddoktaOrMerchantMapping> UserWiseUddoktaOrMerchantMappingRepo;
        public ExternalAuditConfigManager(IRepository<ExternalAuditConfig> externalAuditConfigRepo, IRepository<ExternalAuditMaster> externalAuditMasterRepo, IRepository<UserWiseUddoktaOrMerchantMapping> userWiseUddoktaOrMerchantMappingRepo)
        {
            ExternalAuditConfigRepo = externalAuditConfigRepo;
            ExternalAuditMasterRepo = externalAuditMasterRepo;
            UserWiseUddoktaOrMerchantMappingRepo = userWiseUddoktaOrMerchantMappingRepo;
        }

        public async Task<List<Dictionary<string, object>>> GetAll()
        {
            string sql = $@"select * from HRMS..ExternalAuditConfig";

            var result = ExternalAuditConfigRepo.GetDataDictCollection(sql);

            return await Task.FromResult(result.ToList());
        }

        public async Task<(bool, string)> SaveChanges(ExternalAuditConfig externalAuditConfig)
        {
            string vMsg = string.Empty;
            var existPending = ExternalAuditMasterRepo.Entities.Where(x => x.ApprovalStatusID != 23).ToList();

            if (existPending.IsNotNull() && existPending.Count > 0)
            {
                return (false, "Can not reset due to exist panding data.");
            }


            var existData = ExternalAuditConfigRepo.Entities.SingleOrDefault(x => x.EACID == externalAuditConfig.EACID).MapTo<ExternalAuditConfig>();
            if (existData.IsNull() || existData.EACID.IsZero() || existData.IsAdded)
            {
                externalAuditConfig.IsActive = true;
                externalAuditConfig.SetAdded();
                vMsg = "Saved Successfully";
            }
            else
            {
                externalAuditConfig.EACID = existData.EACID;
                externalAuditConfig.CreatedBy = existData.CreatedBy;
                externalAuditConfig.CreatedDate = existData.CreatedDate;
                externalAuditConfig.CreatedIP = existData.CreatedIP;
                externalAuditConfig.RowVersion = existData.RowVersion;
                externalAuditConfig.SetModified();
                vMsg = "Updated Successfully";

            }

            SetAuditFields(externalAuditConfig);


            #region Untag All
            List<UserWiseUddoktaOrMerchantMapping> allList = UserWiseUddoktaOrMerchantMappingRepo.GetAllList().MapTo<List<UserWiseUddoktaOrMerchantMapping>>();

            allList.ForEach(s =>
            {
                s.IsTagged = false;
                s.SetModified();
            });

            UserWiseUddoktaOrMerchantMappingRepo.AddRange(allList);
            #endregion

            using (var unitOfWork = new UnitOfWork())
            {
                ExternalAuditConfigRepo.Add(externalAuditConfig);
                unitOfWork.CommitChangesWithAudit();
            }

            return (true, vMsg);
        }


    }
}
