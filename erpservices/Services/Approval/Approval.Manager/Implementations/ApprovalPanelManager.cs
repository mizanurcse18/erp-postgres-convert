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
    public class ApprovalPanelManager : ManagerBase, IApprovalPanelManager
    {

        private readonly IRepository<ApprovalPanel> ApprovalPanelRepo;
        public ApprovalPanelManager(IRepository<ApprovalPanel> approvalPanelRepo)
        {
            ApprovalPanelRepo = approvalPanelRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetApprovalPanelListDic()
        {
            string sql = $@"SELECT APT.*, 
                        CASE WHEN APT.IsParallelApproval = 1 THEN 'Parallel' WHEN  APT.IsDynamicApproval =1 THEN 'Dynamic' ELSE '' END AS PanelType
                        , AP.Name AS APTypeName
                        FROM ApprovalPanel APT
                        LEFT JOIN ApprovalType AP ON APT.APTypeID = AP.APTypeID
                        ORDER BY APT.APPanelID DESC";
            var listDict = ApprovalPanelRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<ApprovalPanelDto> GetApprovalPanel(int APPanelID)
        {
            string sql = $@"SELECT APT.*, AP.Name AS APTypeName
                        FROM ApprovalPanel APT
                        LEFT JOIN ApprovalType AP ON APT.APTypeID = AP.APTypeID
                        WHERE APT.APPanelID={APPanelID}
                        ORDER BY APT.APPanelID DESC";
            var data = ApprovalPanelRepo.GetModelData<ApprovalPanelDto>(sql);

            //var dept = ApprovalPanelRepo.SingleOrDefault(x => x.APPanelID == APPanelID).MapTo<ApprovalPanelDto>();

            return await Task.FromResult(data);
        }


        public async Task Delete(int APPanelID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var approvalPanelEnt = ApprovalPanelRepo.Entities.Where(x => x.APPanelID == APPanelID).FirstOrDefault();

                approvalPanelEnt.SetDeleted();
                ApprovalPanelRepo.Add(approvalPanelEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(ApprovalPanelDto approvalPanelDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = ApprovalPanelRepo.Entities.SingleOrDefault(x => x.APPanelID == approvalPanelDto.APPanelID).MapTo<ApprovalPanel>();

                if (existUser.IsNull() || approvalPanelDto.APPanelID.IsZero() )
                {
                    approvalPanelDto.SetAdded();
                    SetNewUserID(approvalPanelDto);
                }
                else
                {
                    approvalPanelDto.SetModified();
                }

                var approvalPanelEnt = approvalPanelDto.MapTo<ApprovalPanel>();
                SetAuditFields(approvalPanelEnt);
                ApprovalPanelRepo.Add(approvalPanelEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(ApprovalPanelDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ApprovalPanel", AppContexts.User.CompanyID);
            obj.APPanelID = code.MaxNumber;
        }

    }
}
