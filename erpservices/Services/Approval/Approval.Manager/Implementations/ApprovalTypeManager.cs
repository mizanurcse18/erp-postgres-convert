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
    public class ApprovalTypeManager : ManagerBase, IApprovalTypeManager
    {

        private readonly IRepository<ApprovalType> ApprovalTypeRepo;
        public ApprovalTypeManager(IRepository<ApprovalType> approvalTypeRepo)
        {
            ApprovalTypeRepo = approvalTypeRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetApprovalTypeListDic()
        {
            string sql = $@"SELECT APT.*
	                        --,CASE 
		                       -- WHEN RG.ApprovalTypeID IS NULL
			                      --  THEN CAST(1 AS BIT)
		                       -- ELSE CAST(0 AS BIT)
		                       -- END IsRemovable
                        FROM ApprovalType APT
                        --LEFT JOIN (
	                       -- SELECT DISTINCT ApprovalTypeID
	                       -- FROM Region
	                       -- ) RG ON APT.ApprovalTypeID = RG.ApprovalTypeID
                        ORDER BY APT.APTypeID DESC";
            var listDict = ApprovalTypeRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<ApprovalTypeDto> GetApprovalType(int APTypeID)
        {

            var dept = ApprovalTypeRepo.SingleOrDefault(x => x.APTypeID == APTypeID).MapTo<ApprovalTypeDto>();

            return await Task.FromResult(dept);
        }


        public async Task Delete(int APTypeID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var approvalTypeEnt = ApprovalTypeRepo.Entities.Where(x => x.APTypeID == APTypeID).FirstOrDefault();

                approvalTypeEnt.SetDeleted();
                ApprovalTypeRepo.Add(approvalTypeEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(ApprovalTypeDto approvalTypeDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = ApprovalTypeRepo.Entities.SingleOrDefault(x => x.APTypeID == approvalTypeDto.APTypeID).MapTo<ApprovalType>();

                if (existUser.IsNull() || approvalTypeDto.APTypeID.IsZero() )
                {
                    approvalTypeDto.SetAdded();
                    SetNewUserID(approvalTypeDto);
                }
                else
                {
                    approvalTypeDto.SetModified();
                }

                var approvalTypeEnt = approvalTypeDto.MapTo<ApprovalType>();
                SetAuditFields(approvalTypeEnt);
                ApprovalTypeRepo.Add(approvalTypeEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(ApprovalTypeDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ApprovalType", AppContexts.User.CompanyID);
            obj.APTypeID = code.MaxNumber;
        }

    }
}
