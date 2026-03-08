using Core;
using Manager.Core.CommonDto;
using Approval.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Manager.Interfaces
{
    public interface IDOAManager
    {
        Task<(bool, string)> SaveChanges(DOADto MR);

        GridModel GetDOAList(GridParameter parameters);
        Task<DOAMasterDto> GetDOAMaster(int doamasterid);
        List<Attachments> GetAttachments(int doamasterid);
        Task<List<DOAApprovalPanelEmployeeDto>> GetDOAApprovalPanelEmployee(int doamasterid);
    }
}
