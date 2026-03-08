using Approval.Manager.Dto;
using Core;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Manager.Interfaces
{
    public interface IApprovalPanelEmployeeConfigManager
    {
        GridModel GetListForGrid(GridParameter parameters);
        Task<IEnumerable<Dictionary<string, object>>> GetApprovalPanelEmployeeListDic();
        Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployee(int APPanelID);
        Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeList(int EmployeeID, int APPanelID);
        Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeListByPanelID(int EmployeeID, int APPanelID);
        Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeListForLeaveOld(int EmployeeID, int APPanelID);
        Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeListForLeave(int EmployeeID, int APPanelID, int LeaveTypeID, bool IsLFA, bool IsFestival, decimal Days);
        Task<List<ApprovalPanelEmployeeConfigDto>> GetApprovalPanelEmployeeListForLeaveEncashment();

        Task<ApprovalPanelEmployeeConfigDto> GetApprovalPanelEmployeeSingleInfoForEdit(ApprovalPanelEmployeeConfigDto ape);
        
        Task<ApprovalPanelEmployeeConfigDto> SaveChanges(ApprovalPanelEmployeeConfigDto approvalPanelDto);
        Task<List<ApprovalPanelEmployeeConfigDto>> SaveReorderedList(List<ApprovalPanelEmployeeConfigDto> approvalPanelDto);
        
        Task Delete(int ApprovalPanelEmployeeID );
        Task DeleteCompleteApprovalPanel(int APPanelID, int DivisionID, int DepartmentID);
        Task CopyPanelData(CopyApprovalPanelConfigDto copiedInfo);
        Task SaveReplaceOrProxyForPendingList(ReplaceOrProxyForPendingListDto model);
        
    }
}
