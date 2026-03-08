using Core;
using Manager.Core.CommonDto;
using Approval.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Manager.Interfaces
{
    public interface IDocumentApprovalManager
    {
        Task<(bool, string)> SaveChanges(DocumentApprovalDto MR);
        Task<(bool, string)> SaveChangesForHR(DocumentApprovalDto MR); 

         GridModel GetDocumentApprovalList(GridParameter parameters);
        Task<DocumentApprovalMasterDto> GetDocumentApprovalMaster(int MRID);
        void DeleteDocumentApproval(int DAMID); 
          //Task<DocumentApprovalMasterDto> GetDocumentApprovalMasterHR(int MRID); 
          List<Attachments> GetAttachments(int DocumentApprovalMID);
        List<ManualApprovalPanelEmployeeDto> GetGRNApprovalPanelDefault(int DocumentApprovalMID);
        List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByTemplateID(int id); 
         Task<List<ManualApprovalPanelEmployeeDto>> GetDocumentApprovalApprovalPanelDefault(int DocumentApprovalMID);


        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> GetApprovalCommentHR(int aprovalProcessId); 


        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<List<ComboModel>> GetForwardingMemberListApprovalService(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberListApprovalService(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> ReportForDocumentApprovalFeedback(int DAMID);
        IEnumerable<Dictionary<string, object>> ReportForDocumentApprovalFeedbackHR(int DAMID); 
    }
}
