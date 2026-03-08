using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface IPettyCashPaymentManager
    {

        GridModel GetPettyCashPaymenteAllList(GridParameter parameters);
        GridModel GetAllApprovedReimburseClaimList(GridParameter parameters);

        Task<(bool, string)> SaveChanges(PettyCashPaymentClaimDto expense);
        List<Attachments> GetAttachments(int tvpid, string TableName);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<PettyCashPaymentMasterDto> GetPettyCashPaymentMaster(int PCPMID);
        Task<List<PettyCashPaymentChildDto>> GetPettyCashPaymentChild(int PCPMID);
        IEnumerable<Dictionary<string, object>> ReportForApprovalFeedback(int PCAMID, int ApTypeID);
        Task<List<Dictionary<string, object>>> GetAllExport(string WhereCondition, string FromDate, string ToDate);

        Task<List<PettyCashFilteredData>> GetPettyCashApprovedReimburseClaimData(int InvoiceMasterID, int IPaymentMasterID, int PCPMID);

    }
}
