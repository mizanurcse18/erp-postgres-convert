using Approval.Manager.Dto;
using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Manager.Interfaces
{
    public interface IApprovalPanelForwardEmployeeManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetApprovalPanelForwardEmployeeListDic();
        Task<List<ApprovalPanelForwardEmployeeDto>> GetApprovalPanelForwardEmployee(int APPanelID, int DivisionID, int DepartmentID);
        Task<Dictionary<string, object>> GetApprovalPanelForwardEmployeeSingleInfoForEdit(ApprovalPanelForwardEmployeeDto ape);
        
        Task<ApprovalPanelForwardEmployeeDto> SaveChanges(ApprovalPanelForwardEmployeeDto dto);
        Task<List<ApprovalPanelForwardEmployeeDto>> SaveReorderedList(List<ApprovalPanelForwardEmployeeDto> dto);
        
        Task Delete(int ApprovalPanelForwardEmployeeID);
        Task CopyPanelData(CopyApprovalPanelDto copiedInfo);
        Task DeleteCompleteApprovalPanel(int APPanelID, int DivisionID, int DepartmentID);

    }
}
