using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IBranchInfoManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetBranchInfoListDic();
        Task<Dictionary<string, object>> GetBranchInfo(int branchId);

        void SaveChanges(BranchInfoDto branchinfoDto);
        Task Delete(int BranchInfoID );

    }
}
