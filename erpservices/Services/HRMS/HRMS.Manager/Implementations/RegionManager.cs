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
    public class RegionManager : ManagerBase, IRegionManager
    {

        private readonly IRepository<Region> RegionRepo;
        public RegionManager(IRepository<Region> regionRepo)
        {
            RegionRepo = regionRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetRegionListDic()
        {
            string sql = $@"SELECT RG.*
	                        ,CASE 
		                        WHEN BI.RegionID IS NULL
			                        THEN CAST(1 AS BIT)
		                        ELSE CAST(0 AS BIT)
		                        END IsRemovable
                            ,ClusterName Cluster
                        FROM Region RG
                        LEFT JOIN Cluster C ON C.ClusterID = RG.ClusterID
                        LEFT JOIN (
	                        SELECT DISTINCT RegionID
	                        FROM BranchInfo
	                        ) BI ON RG.RegionID = BI.RegionID
                        ORDER BY RG.RegionID DESC";
            var listDict = RegionRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<Dictionary<string, object>> GetRegion(int RegionID)
        {

            //var reg = RegionRepo.SingleOrDefault(x => x.RegionID == RegionID).MapTo<RegionDto>();
            string sql = $@"SELECT Reg.*, Cl.ClusterName FROM Region Reg 
                            LEFT JOIN Cluster Cl ON Reg.ClusterID = Cl.ClusterID WHERE Reg.RegionID={RegionID}";

            var reg = RegionRepo.GetData(sql);
            return await Task.FromResult(reg);
        }


        public async Task Delete(int RegionID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var regionEnt = RegionRepo.Entities.Where(x => x.RegionID == RegionID).FirstOrDefault();

                regionEnt.SetDeleted();
                RegionRepo.Add(regionEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(RegionDto regionDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = RegionRepo.Entities.SingleOrDefault(x => x.RegionID == regionDto.RegionID && x.ClusterID == regionDto.ClusterID).MapTo<Region>();

                if (existUser.IsNull() || regionDto.RegionID.IsZero() )
                {
                    regionDto.SetAdded();
                    SetNewUserID(regionDto);
                }
                else
                {
                    regionDto.SetModified();
                }

                var regionEnt = regionDto.MapTo<Region>();
                SetAuditFields(regionEnt);
                RegionRepo.Add(regionEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(RegionDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("Region", AppContexts.User.CompanyID);
            obj.RegionID = code.MaxNumber;
        }

    }
}
