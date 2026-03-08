using Core;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IExternalAuditConfigManager
    {
        Task<List<Dictionary<string, object>>> GetAll();
        Task<(bool, string)> SaveChanges(ExternalAuditConfig externalAuditConfig);
        
    }
}
