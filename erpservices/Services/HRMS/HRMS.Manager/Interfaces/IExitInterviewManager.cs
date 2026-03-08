using Core;
using Manager.Core.CommonDto;
using HRMS.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IExitInterviewManager
    {
        Task<(bool, string)> SaveChanges(ExitInterviewDto MR);

        GridModel GetExitInterviewList(GridParameter parameters);
        Task<ExitInterviewMasterDto> GetExitInterviewMaster(int MRID);
        List<Attachment> GetAttachments(int ExitInterviewMID);
        List<ManualApprovalPanelEmployeeDto> GetGRNApprovalPanelDefault(int ExitInterviewMID);
        List<ManualApprovalPanelEmployeeDto> LoadExistingPanelByTemplateID(int id); 
         Task<List<ManualApprovalPanelEmployeeDto>> GetExitInterviewApprovalPanelDefault(int ExitInterviewMID);


        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);


        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<List<ComboModel>> GetForwardingMemberListApprovalService(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberListApprovalService(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> ReportForExitInterviewFeedback(int EEIID);
        Task Delete(int id);
        Task<ExitInterviewTemplateDto> GetExitInterviewTemplate(int MRID);
    }
}
