using Core;
using Manager.Core.CommonDto;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface ISCCManager
    {
        Task<(bool, string)> SaveChanges(SCCDto MR);

        GridModel GetSCCList(GridParameter parameters);
        GridModel GetSCCListAll(GridParameter parameters);
        Task UpdateSCCMasterAfterReset(SCCMasterDto SCC);
        GridModel GetAllSCCList(GridParameter parameters);
        Task<SCCMasterDto> GetSCCMaster(int SCCMID);
        Task<SCCMasterDto> GetSCCMasterAll(int SCCMID);
        Task<SCCMasterDto> GetSCCMasterFromAllList(int SCCMID); 
         Task<List<SCCChildDto>> GetSCCChild(int SCCMID);
         Task<List<SCCChildDto>> GetSCCChildForAllItem(int POMasterID,int SCCMID);
        List<Attachments> GetAttachments(int SCCMID);
        List<Attachments> GetProposedAttachments(int SCCMID);
        List<ManualApprovalPanelEmployeeDto> GetGRNApprovalPanelDefault(int SCCMID);
        Task<List<ManualApprovalPanelEmployeeDto>> GetSCCApprovalPanelDefault(int SCCMID);


        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);


        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> ReportForGRNApprovalFeedback(int POID);
        IEnumerable<Dictionary<string, object>> ReportForSCCApprovalFeedback(int SCCMID);
    }
}
