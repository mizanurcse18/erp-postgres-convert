using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager
{
    public interface ISystemConfigurationManager
    {        
        Task<List<SystemConfigurationDto>> GetSystemConfiguration();
        Task<SystemConfigurationDto> SaveChanges(SystemConfigurationDto systemConfigurationDto);
    }
}
