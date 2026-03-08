using Core;
using Approval.DAL.Entities;
using Approval.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Manager.Interfaces
{
   public interface IDynamicApprovalWindowManager
    {

        Task<DynamicApprovalPanelWindowDto> GetDynamicApprovalWindow(int id);
        Task<(bool, string)> Save(DynamicApprovalPanelWindowDto settings);
        Task<List<DynamicApprovalPanelWindowDto>> GetAll();
    }
}
