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
    public class BranchInfoManager : ManagerBase, IBranchInfoManager
    {

        private readonly IRepository<BranchInfo> BranchInfoRepo;
        public BranchInfoManager(IRepository<BranchInfo> regionRepo)
        {
            BranchInfoRepo = regionRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetBranchInfoListDic()
        {
            string sql = $@"SELECT
                                clus.branch_id AS ""BranchID"",
                                clus.branch_code AS ""BranchCode"",
                                clus.branch_name AS ""BranchName"",
                                CASE 
                                    WHEN emp.branch_id IS NULL
                                        THEN TRUE
                                    ELSE FALSE
                                END AS ""IsRemovable""
                            FROM
                                branch_info clus
                            LEFT JOIN (
                                SELECT DISTINCT
                                    branch_id
                                FROM
                                    employment
                            ) emp ON clus.branch_id = emp.branch_id
                            ORDER BY
                                clus.branch_id DESC";
            var listDict = BranchInfoRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<Dictionary<string, object>> GetBranchInfo(int BranchID)
        {

            //var reg = BranchInfoRepo.SingleOrDefault(x => x.BranchID == BranchID).MapTo<BranchInfoDto>();
            string sql = $@"SELECT Reg.*, Rg.RegionName FROM BranchInfo Reg 
                            LEFT JOIN Region Rg ON Reg.RegionID = Rg.RegionID WHERE Reg.BranchID={BranchID}";

            var reg = BranchInfoRepo.GetData(sql);
            return await Task.FromResult(reg);
        }


        public async Task Delete(int BranchID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var regionEnt = BranchInfoRepo.Entities.Where(x => x.BranchID == BranchID).FirstOrDefault();

                regionEnt.SetDeleted();
                BranchInfoRepo.Add(regionEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(BranchInfoDto regionDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = BranchInfoRepo.Entities.SingleOrDefault(x => x.BranchID == regionDto.BranchID && x.RegionID == regionDto.RegionID).MapTo<BranchInfo>();

                if (existUser.IsNull() || regionDto.BranchID.IsZero() )
                {
                    regionDto.SetAdded();
                    SetNewUserID(regionDto);
                }
                else
                {
                    regionDto.SetModified();
                }

                var regionEnt = regionDto.MapTo<BranchInfo>();
                SetAuditFields(regionEnt);
                BranchInfoRepo.Add(regionEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(BranchInfoDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("BranchInfo", AppContexts.User.CompanyID);
            obj.BranchID = code.MaxNumber;
        }

    }
}
