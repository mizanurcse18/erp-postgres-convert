using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface IPettyCashReimburseManager
    {
       

        GridModel GetPettyCashReimburseAllList(GridParameter parameters);
        Task<List<PettyCashFilteredData>> GetPettyCashReimburseClaimData( int InvoiceMasterID, int IPaymentMasterID, int PCRMID);
        Task<(bool, string)> SaveChanges(PettyCashReimburseClaimDto expense);

        Task<PettyCashReimburseMasterDto> GetPettyCashReimburseMaster(int PCEMID);
        Task<List<PettyCashReimburseChildDto>> GetPettyCashReimburseChild(int PCEMID);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        List<Attachments> GetAttachments(int tvpid, string TableName);
        IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID);


    }
}
