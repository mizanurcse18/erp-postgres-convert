using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface IPettyCashExpenseClaimManager
    {
        GridModel GetPettyCashExpenseClaimList(GridParameter parameters);
        GridModel GetPettyCashExpenseAndAdvanceList(GridParameter parameters);
        Task<PettyCashExpenseMasterDto> GetPettyCashExpenseClaim(int PCEMID);
        Task<List<PettyCashExpenseChildDto>> GetPettyCashExpenseClaimChild(int PCEMID);
        Task<(bool, string)> SaveChanges(PettyCashExpenseClaimDto expense);
        Task RemovePettyCashExpenseClaim(int PCEMID);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID);
        List<Attachments> GetAttachments(int id, string TableName);
    }
}
