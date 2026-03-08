using Approval.Manager.Dto;
using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IApprovalPanelManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetApprovalPanelListDic();
        Task<ApprovalPanelDto> GetApprovalPanel(int ApprovalPanelID);

        void SaveChanges(ApprovalPanelDto approvalPanelDto);
        Task Delete(int ApprovalPanelID );

    }
}
