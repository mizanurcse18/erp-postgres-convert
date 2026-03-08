using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IClusterManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetClusterListDic();
        Task<ClusterDto> GetCluster(int clusterId);

        void SaveChanges(ClusterDto clusterDto);
        Task Delete(int ClusterID );

    }
}
