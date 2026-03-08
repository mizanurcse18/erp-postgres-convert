using Core;
using HRMS.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IDocumentUploadManager
    {
        Task<DocumentUploadDto> GetDocumentUpload(int DUID,int ApprovalProcessID);
        Task<(bool, string)> SaveChanges(DocumentUploadDto documentUpload);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByDUID(int id);

        List<Attachments> GetAttachments(int DUID);
        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<DocumentUploadDto> GetDocumentUploadForReAssessment(int DUID);
        Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForDocumentUpload();
        Task<IEnumerable<Dictionary<string, object>>> GetAllHODDocumentUploadList();

        //Report
        IEnumerable<Dictionary<string, object>> ReportForDocumentUploadAttachments(int DUID);
        Dictionary<string, object> ReportForDocumentUploadMaster(int DUID);
        IEnumerable<Dictionary<string, object>> ReportForEADApprovalFeedback(int DUID);
        Task RemoveDocumentUpload(int DUID, int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        #region Grid
        GridModel GetListForGrid(GridParameter parameters);
        #endregion
        void UpdateDocumentUploadStatus(DocumentUploadResponseDto documentUploadResponseDto);
    }
}
