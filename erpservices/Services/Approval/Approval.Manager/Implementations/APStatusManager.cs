using Approval.DAL.Entities;
using Approval.Manager.Dto;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class APStatusManager : ManagerBase, IAPStatusManager
    {

        private readonly IRepository<APStatus> APStatusRepo;
        public APStatusManager(IRepository<APStatus> aPStatusRepo)
        {
            APStatusRepo = aPStatusRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetAPStatusListDic()
        {
            string sql = $@"SELECT APT.*
	                        --,CASE 
		                       -- WHEN RG.APStatusID IS NULL
			                      --  THEN CAST(1 AS BIT)
		                       -- ELSE CAST(0 AS BIT)
		                       -- END IsRemovable
                        FROM APStatus APT
                        --LEFT JOIN (
	                       -- SELECT DISTINCT APStatusID
	                       -- FROM Region
	                       -- ) RG ON APT.APStatusID = RG.APStatusID
                        ORDER BY APT.APStatusID DESC";
            var listDict = APStatusRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<APStatusDto> GetAPStatus(int APStatusID)
        {

            var dept = APStatusRepo.SingleOrDefault(x => x.APStatusID == APStatusID).MapTo<APStatusDto>();

            return await Task.FromResult(dept);
        }


        public async Task Delete(int APStatusID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var aPStatusEnt = APStatusRepo.Entities.Where(x => x.APStatusID == APStatusID).FirstOrDefault();

                aPStatusEnt.SetDeleted();
                APStatusRepo.Add(aPStatusEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(APStatusDto aPStatusDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = APStatusRepo.Entities.SingleOrDefault(x => x.APStatusID == aPStatusDto.APStatusID).MapTo<APStatus>();

                if (existUser.IsNull() || aPStatusDto.APStatusID.IsZero() )
                {
                    aPStatusDto.SetAdded();
                    SetNewUserID(aPStatusDto);
                }
                else
                {
                    aPStatusDto.SetModified();
                }

                var aPStatusEnt = aPStatusDto.MapTo<APStatus>();
                SetAuditFields(aPStatusEnt);
                APStatusRepo.Add(aPStatusEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(APStatusDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("APStatus", AppContexts.User.CompanyID);
            obj.APStatusID = code.MaxNumber;
        }

    }
}
