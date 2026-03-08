using Core;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface ILeaveEncashmentApplicationManager
    {
        Task<LeaveBalanceAndDetailsResponse> GetAnnualLeaveBalanceAndDetails();
        Task<(bool, string)> SaveChanges(LeaveEncashmentApplication application);
        GridModel GetLeaveEncashmentApplicationList(GridParameter parameters);
        GridModel GetLeaveEncashmentApplicationListForHODApproval(GridParameter parameters);
        Task<LeaveEncashmentApplication> GetLeaveEncashmentApplication(int employeeLeaveAID, int approvalProcessID);
        Task<LeaveApplication> GetLeaveEncashmentApplicationForAdmin(int employeeLeaveAID, int approvalProcessID);
        Task<LeaveEncashmentApplicationWithComments> GetLeaveEncashmentApplicationWithCommentsForApproval(int employeeLeaveAID, int approvalProcessID);
        Task<LeaveApplicationWithComments> GetLeaveEncashmentApplicationWithCommentsForApprovalForHR(int employeeLeaveAID, int approvalProcessID);
        GridModel GetAllLeaveEncashmentApplicationList(GridParameter parameters);
        GridModel GetAllLeaveEncashmentApplicationListForHR(GridParameter parameters, string type);
        GridModel GetAllLeaveEncashmentApplicationListForDashboard(GridParameter parameters);
        Task<LeavePolicySettingsDto> GetLeavePolicySettings(int leaveCategoryId);
        Task<(bool, string)> SavePolicySettings(LeavePolicySettingsDto settings);
        bool CheckMultipleSupervisor();
        bool CheckEncashmentEligible();
        Task<List<LeavePolicySettingsDto>> GetLeaveCategoriesWithSettings();
        GridModel GetAllLEApplicationList(GridParameter parameters);
        Task<DataSet> GetAllLeaveEncashmentApplicationList(int ALEWMasterID);
        Task<List<Dictionary<string, object>>> GetAllLeaveEncashmentApplications(int ALEWMasterID);

    }
}
