using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface ISupportRequisitionManager
    {
        #region Grid
        Task<GridModel> GetListForGrid(GridParameter parameters);
        Task<GridModel> GetAllListForGrid(GridParameter parameters);
        Task<GridModel> GetListForGridForEmp(GridParameter parameters);
        #endregion
        Task<SupportRequisitionDto> GetSupportRequisition(int SRMID,int ApprovalProcessID);
        Task<(bool, string)> SaveChanges(SupportRequisitionDto RSM);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByRSMID(int id);
        //Task<List<AccessoriesItemDetailsDto>> GetAccessoriesItemDetails(int SRMID);
        Task<List<AssetItemDetailsDto>> GetAssetItemDetails(int SRMID);
        Task<List<AccessRequestDetailsDto>> GetAccessDetails(int SRMID);
        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<SupportRequisitionDto> GetSupportRequisitionForReAssessment(int SRMID);
        Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForSupportRequisition();
        //Report
        IEnumerable<Dictionary<string, object>> ReportForSupportRequisitionAttachments(int SRMID);
        Dictionary<string, object> ReportForSupportRequisitionMaster(int SRMID);
        IEnumerable<Dictionary<string, object>> ReportForRSMApprovalFeedback(int SRMID);
        IEnumerable<Dictionary<string, object>> EmployeeApprovalMemberFeedbackForRSM(int SRMID, int ApprovalProcessID);
        Task RemoveSupportRequisition(int SRMID);
        Task<(bool, string)> SettleSupportRequisition(int SRMID, string SettleSupportRequisition);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        Task<List<Dictionary<string, object>>> DownloadSupportRequisition();

        Task<List<Dictionary<string, object>>> GetAllSupportRequestListByWhereCondition(string wherecondition, string FromDate, string ToDate);
        Task<List<Dictionary<string, object>>> GetAllSupportRequestList(string wherecondition, string FromDate, string ToDate);


    }
}
