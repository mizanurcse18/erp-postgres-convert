using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IAuditApprovalManager
    {
        Task<(bool, string)> Save(List<AuditApprovalDto> settings);
        Task<List<AuditApprovalDto>> GetAll();
    }
}
