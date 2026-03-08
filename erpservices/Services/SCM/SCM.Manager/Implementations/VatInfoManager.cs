using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class VatInfoManager : ManagerBase, IVatInfoManager
    {

        private readonly IRepository<VatInfo> VatInfoRepo;
        public VatInfoManager(IRepository<VatInfo> vatInfoRepo)
        {
            VatInfoRepo = vatInfoRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetVatInfoListDic()
        {
            string sql = $@"SELECT 
	                            VatInfoID AS value,
	                            VatInfoID,
	                            VatPercent,
	                            VatPolicies,
	                            IsRebateable,
	                            RebatePercentage,
	                            'Vat Percent: '+VatPolicies+' , Rebateable: '+ CASE WHEN IsRebateable = 0 THEN 'No' ELSE 'YES' END +
	                            ', Rebateable: '+Cast(RebatePercentage as nvarchar(100)) AS label
                            FROM 
	                            VatInfo 
                            ORDER BY VatPercent";
            var listDict = VatInfoRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public async Task<Dictionary<string, object>> GetVatInfo(int VatInfoID)
        {

            string sql = $@"SELECT 
	                            VatInfoID AS value,
	                            VatInfoID,
	                            VatPercent,
	                            VatPolicies,
	                            IsRebateable,
	                            RebatePercentage,
	                            'Vat Percent: '+VatPolicies+' , Rebateable: '+ CASE WHEN IsRebateable = 0 THEN 'No' ELSE 'YES' END +
	                            ', Rebateable: '+Cast(RebatePercentage as nvarchar(100)) AS label
                            FROM 
	                            VatInfo
                            WHERE VatInfoID={VatInfoID}";

            var reg = VatInfoRepo.GetData(sql);
            return await Task.FromResult(reg);
        }


        public async Task Delete(int VatInfoID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var VatInfoEnt = VatInfoRepo.Entities.Where(x => x.VatInfoID == VatInfoID).FirstOrDefault();

                VatInfoEnt.SetDeleted();
                VatInfoRepo.Add(VatInfoEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public Task<VatInfoDto> SaveChanges(VatInfoDto VatInfoDto)
        {

            using (var unitOfWork = new UnitOfWork())
            {
                var existVatInfo = VatInfoRepo.Entities.SingleOrDefault(x => x.VatInfoID == VatInfoDto.VatInfoID).MapTo<VatInfo>();
                if (existVatInfo.IsNull() || VatInfoDto.VatInfoID.IsZero() )
                {
                    VatInfoDto.SetAdded();
                    SetNewUserID(VatInfoDto);
                }
                else
                {

                    VatInfoDto.CreatedBy = existVatInfo.CreatedBy;
                    VatInfoDto.CreatedDate = existVatInfo.CreatedDate;
                    VatInfoDto.CreatedIP = existVatInfo.CreatedIP;
                    VatInfoDto.RowVersion = existVatInfo.RowVersion;
                    VatInfoDto.SetModified();
                }

                var VatInfoEnt = VatInfoDto.MapTo<VatInfo>();
                SetAuditFields(VatInfoEnt);
                VatInfoRepo.Add(VatInfoEnt);
                unitOfWork.CommitChangesWithAudit();
            }
            return Task.FromResult(VatInfoDto);
        }

        private void SetNewUserID(VatInfoDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("VatInfo", AppContexts.User.CompanyID);
            obj.VatInfoID = code.MaxNumber;
        }

    }
}
