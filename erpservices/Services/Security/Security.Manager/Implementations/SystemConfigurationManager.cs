using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Extension;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Security.DAL;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Manager
{
    public class SystemConfigurationManager : ManagerBase, ISystemConfigurationManager
    {
        private readonly IRepository<SystemConfiguration> SystemConfigurationRepo;
        private readonly IRepository<Period> PeriodRepo;
        public SystemConfigurationManager(IRepository<SystemConfiguration> systemConfigurationRepo
            )
        {
            SystemConfigurationRepo = systemConfigurationRepo;
        }


        public async Task<List<SystemConfigurationDto>> GetSystemConfiguration()
        {
            var sysConfiguration = await SystemConfigurationRepo.GetAll();
            return sysConfiguration.Select(config => config.MapTo<SystemConfigurationDto>()).ToList();
        }

        public async Task<SystemConfigurationDto> SaveChanges(SystemConfigurationDto systemConfigurationDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existMaster = SystemConfigurationRepo.Entities.SingleOrDefault(x => x.SystemConfigurationID == systemConfigurationDto.SystemConfigurationID).MapTo<SystemConfigurationDto>();
                if (
                    systemConfigurationDto.SystemConfigurationID.IsZero() || systemConfigurationDto.IsAdded)
                {
                    systemConfigurationDto.SetAdded();
                    SetNewId(systemConfigurationDto);
                }
                else
                {
                    systemConfigurationDto.CreatedBy = existMaster.CreatedBy;
                    systemConfigurationDto.CreatedDate = existMaster.CreatedDate;
                    systemConfigurationDto.CreatedIP = existMaster.CreatedIP;
                    systemConfigurationDto.RowVersion = existMaster.RowVersion;
                    systemConfigurationDto.SetModified();
                }
                var masterEnt = systemConfigurationDto.MapTo<SystemConfiguration>();
                SetAuditFields(masterEnt);

                SystemConfigurationRepo.Add(masterEnt);
                
                unitOfWork.CommitChangesWithAudit();

                systemConfigurationDto = masterEnt.MapTo<SystemConfigurationDto>();
                masterEnt.MapToAuditFields(systemConfigurationDto);
            }
            await Task.CompletedTask;

            return systemConfigurationDto;
        }

        private void SetNewId(SystemConfigurationDto systemConfigurationDto)
        {
            if (!systemConfigurationDto.IsAdded) return;
            var code = GenerateSystemCode("SystemConfiguration", AppContexts.User.CompanyID);
            systemConfigurationDto.SystemConfigurationID = code.MaxNumber;
        }


    }
}
