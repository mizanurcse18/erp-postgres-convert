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
    public class WageCodeConfigurationManager : ManagerBase, IWageCodeConfigurationManager
    {

        private readonly IRepository<WageCodeConfiguration> WageCodeConfigurationRepo;
        public WageCodeConfigurationManager(IRepository<WageCodeConfiguration> wageCodeConfigurationRepo)
        {
            WageCodeConfigurationRepo = wageCodeConfigurationRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetWageCodeConfigurationListDic()
        {
            string sql = $@"SELECT wcc.*, sv.SystemVariableCode as TypeName
                        FROM WageCodeConfiguration wcc
                        LEFT JOIN Security..SystemVariable sv ON sv.SystemVariableID=wcc.TypeID
                        ORDER BY wcc.WageCodeConfigurationID DESC";
            var listDict = WageCodeConfigurationRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<Dictionary<string, object>> GetWageCodeConfiguration(int WageCodeConfigurationID)
        {
            string sql = $@"SELECT wcc.*, sv.SystemVariableCode as TypeName
                        FROM WageCodeConfiguration wcc
                        LEFT JOIN Security..SystemVariable sv ON sv.SystemVariableID=wcc.TypeID
                        WHERE wcc.WageCodeConfigurationID={WageCodeConfigurationID}";

            var wcc = WageCodeConfigurationRepo.GetData(sql);
            return await Task.FromResult(wcc);

            //var dept = WageCodeConfigurationRepo.SingleOrDefault(x => x.WageCodeConfigurationID == WageCodeConfigurationID).MapTo<WageCodeConfigurationDto>();

            //return await Task.FromResult(dept);
        }


        public async Task Delete(int WageCodeConfigurationID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var WageCodeConfigurationEnt = WageCodeConfigurationRepo.Entities.Where(x => x.WageCodeConfigurationID == WageCodeConfigurationID).FirstOrDefault();

                WageCodeConfigurationEnt.SetDeleted();
                WageCodeConfigurationRepo.Add(WageCodeConfigurationEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(WageCodeConfigurationDto WageCodeConfigurationDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = WageCodeConfigurationRepo.Entities.SingleOrDefault(x => x.WageCodeConfigurationID == WageCodeConfigurationDto.WageCodeConfigurationID).MapTo<WageCodeConfiguration>();

                if (existUser.IsNull() || WageCodeConfigurationDto.WageCodeConfigurationID.IsZero() )
                {
                    WageCodeConfigurationDto.SetAdded();
                    SetNewUserID(WageCodeConfigurationDto);
                }
                else
                {
                    WageCodeConfigurationDto.SetModified();
                }

                var WageCodeConfigurationEnt = WageCodeConfigurationDto.MapTo<WageCodeConfiguration>();
                SetAuditFields(WageCodeConfigurationEnt);
                WageCodeConfigurationRepo.Add(WageCodeConfigurationEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(WageCodeConfigurationDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("WageCodeConfiguration", AppContexts.User.CompanyID);
            obj.WageCodeConfigurationID = code.MaxNumber;
        }

    }
}
