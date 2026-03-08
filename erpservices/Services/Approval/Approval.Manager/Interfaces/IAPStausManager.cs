using Approval.Manager.Dto;
using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IAPStatusManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetAPStatusListDic();
        Task<APStatusDto> GetAPStatus(int APStatusID);

        void SaveChanges(APStatusDto APStatusDto);
        Task Delete(int APStatusID );

    }
}
