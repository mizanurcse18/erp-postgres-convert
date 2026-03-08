using Approval.Manager.Dto;
using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Manager.Interfaces
{
    public interface IApprovalPanelEmployeeManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetApprovalPanelEmployeeListDic();
        Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployee(int APPanelID, int DivisionID, int DepartmentID);
        Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeList(int EmployeeID, int APPanelID);
        Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListForStNfa(int EmployeeID, int APPanelID,int TemplateID);
        Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListByPanelID(int EmployeeID, int APPanelID);
        Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListForLeaveOld(int EmployeeID, int APPanelID);
        Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListForLeave(int EmployeeID, int APPanelID, int LeaveTypeID, bool IsLFA, bool IsFestival, decimal Days);
        Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListForLeaveEncashment();

        Task<ApprovalPanelEmployeeDto> GetApprovalPanelEmployeeSingleInfoForEdit(ApprovalPanelEmployeeDto ape);
        
        Task<ApprovalPanelEmployeeDto> SaveChanges(ApprovalPanelEmployeeDto approvalPanelDto);
        Task<List<ApprovalPanelEmployeeDto>> SaveReorderedList(List<ApprovalPanelEmployeeDto> approvalPanelDto);
        
        Task Delete(int ApprovalPanelEmployeeID );
        Task DeleteCompleteApprovalPanel(int APPanelID, int DivisionID, int DepartmentID);
        Task CopyPanelData(CopyApprovalPanelDto copiedInfo);
        Task SaveReplaceOrProxyForPendingList(ReplaceOrProxyForPendingListDto model);
        Task<List<ApprovalPanelEmployeeDto>> GetApprovalPanelEmployeeListByMinMaxLimit(int EmployeeID, int APTypeID, double total);
        Task<List<ApprovalPanelEmployeeDto>> GetDynamicApprovalPanelEmployeeList(int EmployeeID, int APTypeID, decimal Amount);

    }
}
