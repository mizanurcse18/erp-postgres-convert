using Approval.Manager.Dto;
using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IApprovalTypeManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetApprovalTypeListDic();
        Task<ApprovalTypeDto> GetApprovalType(int ApprovalTypeID);

        void SaveChanges(ApprovalTypeDto approvalTypeDto);
        Task Delete(int ApprovalTypeID );

    }
}
