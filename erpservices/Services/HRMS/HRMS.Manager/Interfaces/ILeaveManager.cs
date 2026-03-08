using Core;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
   public interface ILeaveManager
    {
        Task<LeaveBalanceAndDetailsResponse> GetLeaveBalanceAndDetails(int leaveTypeId, DateTime startDate, DateTime endDate,int employeeLeaveAID);
        Task<LeaveBalanceAndDetailsResponse> GetLeaveBalanceAndDetailsHr(int leaveTypeId, DateTime startDate, DateTime endDate,int employeeLeaveAID, int employeeID);
        Task<NotificationResponseDto> SaveChanges(LeaveApplication application);
        GridModel GetLeaveApplicationList(GridParameter parameters);
        Task<LeaveApplication> GetLeaveApplication(int employeeLeaveAID,int approvalProcessID);
        Task<LeaveApplication> GetLeaveApplicationHr(int employeeLeaveAID,int employeeID);
        Task<LeaveApplication> GetLeaveApplicationForAdmin(int employeeLeaveAID,int approvalProcessID);
        Task<NotificationResponseDto> RemovLeaveApplicationAsync(int employeeLeaveAID);
        Task<LeaveApplicationWithComments> GetLeaveApplicationWithCommentsForApproval(int employeeLeaveAID, int approvalProcessID);
        Task<LeaveApplicationWithComments> GetLeaveApplicationWithCommentsForApprovalForHR(int employeeLeaveAID, int approvalProcessID); 

        void SaveLFA(LFADeclarationDto application);
        GridModel GetAllLeaveApplicationList(GridParameter parameters);
        GridModel GetAllPendingLeaveApplicationList(GridParameter parameters);
        GridModel GetAllLeaveApplicationListForHR(GridParameter parameters, string type);
        GridModel GetAllLeaveApplicationListForHROnbehalfOfEmployee(GridParameter parameters, string type);
        GridModel GetAllLeaveApplicationListForDashboard(GridParameter parameters);
        Task<LeavePolicySettingsDto> GetLeavePolicySettings(int leaveCategoryId);
        Task<(bool, string)> SavePolicySettings(LeavePolicySettingsDto settings);
        bool CheckMultipleSupervisor(int EmployeeID); 
         Task<List<LeavePolicySettingsDto>> GetLeaveCategoriesWithSettings(int EmployeeLeaveAID = 0);
        Task<NotificationResponseDto> CancelLeaveApplication(LeaveApplication application);
        //Task<IEnumerable<Dictionary<string, object>>> GetLeaveDetails();
        Task<IEnumerable<Dictionary<string, object>>> GetLeaveDetailsForHr(int EmployeeID);
        Task<IEnumerable<Dictionary<string, object>>> GetHolidaysWorkDetails();
        Task<IEnumerable<Dictionary<string, object>>> GetHolidaysWorkDetailsAll();
        Task<IEnumerable<Dictionary<string, object>>> GetHolidaysWorkDetailsById(int EmployeeID);
        Task<IEnumerable<Dictionary<string, object>>> GetHolidaysWorkDetailsAllById(int EmployeeID);
        Task<IEnumerable<Dictionary<string, object>>> GetUnauthorizedLeave(DateTime fromDate, DateTime toDate, int DivisionID, int DepartmentID);
        Task<IEnumerable<Dictionary<string, object>>> GetUnauthorizedLeaveHr(int employeeID);
        Task<IEnumerable<Dictionary<string, object>>> GetUnauthorizedLeaveViewHr(int EmployeeLeaveAID);
        Task<(bool, string)> SaveEmailNotification(List<UnauthorizedLeaveEmailNotificationDto> unauthorizedLeavs);


    }
}
