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
            string sql = $@"SELECT
                                rg.region_id AS ""RegionID"",
                                rg.cluster_id AS ""ClusterID"",
                                rg.region_code AS ""RegionCode"",
                                rg.region_name AS ""RegionName"",
                                CASE 
                                    WHEN bi.region_id IS NULL
                                        THEN TRUE
                                    ELSE FALSE
                                END AS ""IsRemovable"",
                                c.cluster_name AS ""Cluster""
                            FROM
                                region rg
                            LEFT JOIN
                                cluster c ON c.cluster_id = rg.cluster_id
                            LEFT JOIN (
                                SELECT DISTINCT
                                    region_id
                                FROM
                                    branch_info
                            ) bi ON rg.region_id = bi.region_id
                            ORDER BY
                                rg.region_id DESC";
            var listDict = RegionRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<Dictionary<string, object>> GetRegion(int RegionID)
        {

            //var reg = RegionRepo.SingleOrDefault(x => x.RegionID == RegionID).MapTo<RegionDto>();
            string sql = $@"SELECT
                                reg.region_id AS ""RegionID"",
                                reg.cluster_id AS ""ClusterID"",
                                reg.region_code AS ""RegionCode"",
                                reg.region_name AS ""RegionName"",
                                cl.cluster_name AS ""ClusterName""
                            FROM
                                region reg
                            LEFT JOIN
                                cluster cl ON reg.cluster_id = cl.cluster_id
                            WHERE
                                reg.region_id = {RegionID}";

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
