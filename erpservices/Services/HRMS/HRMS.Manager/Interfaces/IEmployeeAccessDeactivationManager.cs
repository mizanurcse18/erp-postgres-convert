using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IEmployeeAccessDeactivationManager
    {
        Task<EmployeeAccessDeactivationDto> GetAccessDeactivation(int EADID,int ApprovalProcessID);
        Task<EmployeeAccessDeactivationDto> GetAccessDeactivationDivisionClearence(int EADID,int ApprovalProcessID);
        Task<(bool, string)> SaveChanges(EmployeeAccessDeactivationDto EAD);
        Task<(bool, string)> SaveChangesDivisionClearence(EmployeeAccessDeactivationDto EAD);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> GetApprovalCommentDivClearence(int aprovalProcessId);

        Task<List<ManualApprovalPanelEmployeeDto>> GetDivisionClearenceApprovalPanelDefault(int EADID);
        List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByEADID(int id);

        List<Attachments> GetAttachments(int EADID);
        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<EmployeeAccessDeactivationDto> GetAccessDeactivationForReAssessment(int EADID);
        Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForAccessDeactivation();

        //Report
        IEnumerable<Dictionary<string, object>> ReportForAccessDeactivationAttachments(int RefID, string TableName);
        Dictionary<string, object> ReportForAccessDeactivationMaster(int EADID);
        IEnumerable<Dictionary<string, object>> ReportForEADApprovalFeedback(int EADID);
        IEnumerable<Dictionary<string, object>> EmployeeApprovalMemberFeedbackForEAD(int EADID, int ApprovalProcessID);
        IEnumerable<Dictionary<string, object>> ReportForDivClearenceApprovalFeedback(int EADID);
        Task RemoveAccessDeactivation(int EADID, int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        Task<List<Dictionary<string, object>>> DownloadAccessDeactivation();


        #region Grid
        Task<GridModel> GetListForGrid(GridParameter parameters);
        Task<GridModel> GetDivClearenceListForGrid(GridParameter parameters);
        #endregion
    }
}
