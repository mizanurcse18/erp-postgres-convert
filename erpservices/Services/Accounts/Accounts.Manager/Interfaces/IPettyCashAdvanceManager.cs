using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface IPettyCashAdvanceManager
    {
        Task<List<PettyCashAdvanceMasterDto>> GetPettyCashAdvanceList();
        Task<PettyCashAdvanceMasterDto> GetPettyCashAdvance(int PCAMID); 
        GridModel GetPettyCashAdvanceClaimList(GridParameter parameters);
        Task<List<PettyCashAdvanceChildDto>> GetPettyCashAdvanceChild(int PCAMID);
        Task<List<PettyCashAdvanceChildDto>> GetPettyCashAdvanceResubmitChild(int PCAMID);
        Task<(bool, string)> SaveChanges(PettyCashAdvanceDto iou);
        
        Task RemovePettyCashAdvanceMaster(int PCAMID, int ApprovalProcessID);

        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> GetResubmitApprovalComment(int aprovalProcessId);

        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> ReportForPCAApprovalFeedback(int PCAMID, int ApTypeID);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);

        GridModel GetPettyCashAdvanceResubmitClaimList(GridParameter parameters);
        Task<(bool, string)> SaveChangesResubmit(PettyCashAdvanceDto iou);
        Task<PettyCashAdvanceMasterDto> GetPettyCashAdvanceResubmit(int PCAMID,int ApprovalProcessID);
        List<Attachments> GetAttachments(int id, string TableName);

    }
}
