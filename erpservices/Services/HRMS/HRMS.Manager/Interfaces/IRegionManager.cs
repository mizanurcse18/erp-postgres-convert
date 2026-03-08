using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IRegionManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetRegionListDic();
        Task<Dictionary<string, object>> GetRegion(int clusterId);

        void SaveChanges(RegionDto clusterDto);
        Task Delete(int RegionID );

    }
}
