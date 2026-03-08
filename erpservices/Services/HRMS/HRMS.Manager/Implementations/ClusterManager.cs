using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
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
    public class ClusterManager : ManagerBase, IClusterManager
    {

        private readonly IRepository<Cluster> ClusterRepo;
        public ClusterManager(IRepository<Cluster> clusterRepo)
        {
            ClusterRepo = clusterRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetClusterListDic()
        {
            string sql = $@"SELECT Clus.*
	                        ,CASE 
		                        WHEN RG.ClusterID IS NULL
			                        THEN CAST(1 AS BIT)
		                        ELSE CAST(0 AS BIT)
		                        END IsRemovable
                        FROM Cluster Clus
                        LEFT JOIN (
	                        SELECT DISTINCT ClusterID
	                        FROM Region
	                        ) RG ON Clus.ClusterID = RG.ClusterID
                        ORDER BY Clus.ClusterID DESC";
            var listDict = ClusterRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<ClusterDto> GetCluster(int ClusterID)
        {

            var dept = ClusterRepo.SingleOrDefault(x => x.ClusterID == ClusterID).MapTo<ClusterDto>();

            return await Task.FromResult(dept);
        }


        public async Task Delete(int ClusterID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var clusterEnt = ClusterRepo.Entities.Where(x => x.ClusterID == ClusterID).FirstOrDefault();

                clusterEnt.SetDeleted();
                ClusterRepo.Add(clusterEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(ClusterDto clusterDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = ClusterRepo.Entities.SingleOrDefault(x => x.ClusterID == clusterDto.ClusterID).MapTo<Cluster>();

                if (existUser.IsNull() || clusterDto.ClusterID.IsZero() )
                {
                    clusterDto.SetAdded();
                    SetNewUserID(clusterDto);
                }
                else
                {
                    clusterDto.SetModified();
                }

                var clusterEnt = clusterDto.MapTo<Cluster>();
                SetAuditFields(clusterEnt);
                ClusterRepo.Add(clusterEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(ClusterDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Cluster", AppContexts.User.CompanyID);
            obj.ClusterID = code.MaxNumber;
        }

    }
}
