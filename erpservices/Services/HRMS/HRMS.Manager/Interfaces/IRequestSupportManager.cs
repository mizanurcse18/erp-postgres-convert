using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IRequestSupportManager
    {
        #region Grid
        Task<GridModel> GetListForGrid(GridParameter parameters);

        Task<GridModel> GetListForGridForEmp(GridParameter parameters);
        #endregion
        Task<RequestSupportDto> GetRequestSupport(int RSMID,int ApprovalProcessID);
        Task<(bool, string)> SaveChanges(RequestSupportDto RSM);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByRSMID(int id);
        //List<Attachments> GetAttachments(int RSMID);
        Task<List<ItemDetailsDto>> GetitemDetails(int RSMID);
        Task<List<VehicleDetailsDto>> GetVehicleDetails(int RSMID);
        Task<List<FacilitiesDetailsDto>> GetFacilitiesDetails(int RSMID);
        Task<List<RenovationORMaintenanceDetailsDto>> GetRenovationORMaintenanceDetails(int RSMID);
        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<RequestSupportDto> GetRequestSupportForReAssessment(int RSMID);
        Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForRequestSupport();
        //Report
        IEnumerable<Dictionary<string, object>> ReportForRequestSupportAttachments(int RSMID);
        Dictionary<string, object> ReportForRequestSupportMaster(int RSMID);
        IEnumerable<Dictionary<string, object>> ReportForRSMApprovalFeedback(int RSMID);
        IEnumerable<Dictionary<string, object>> EmployeeApprovalMemberFeedbackForRSM(int RSMID, int ApprovalProcessID);
        Task RemoveRequestSupport(int RSMID);
        Task<(bool, string)> SettleRequestSupport(int RSMID, string SettleRequestSupport);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        Task<List<Dictionary<string, object>>> DownloadRequestSupport();

        Task<List<Dictionary<string, object>>> GetAllSupportRequestListByWhereCondition(string wherecondition, string FromDate, string ToDate);
        GridModel GetAllListForGrid(GridParameter parameters);

    }
}
