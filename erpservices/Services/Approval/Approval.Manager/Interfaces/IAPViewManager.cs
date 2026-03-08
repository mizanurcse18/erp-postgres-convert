using Approval.Manager.Dto;
using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IAPViewManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetAPViewListDic(int APTypeID, int ReferenceID);
        Task<IEnumerable<Dictionary<string, object>>> GetAPViewForAll(int APTypeID, int ReferenceID); 

    }
}
