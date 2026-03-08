using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface ITaxationVettingPaymentManager
    {
        Task<(bool, string)> SaveChanges(TaxationVettingPaymentDto model);
        GridModel GetListForGrid(GridParameter parameters);
        List<Attachments> GetAttachments(int tvpid,string TableName);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        Task<Dictionary<string, object>> TaxationVettingAndInvoiceInfo(int tvmid);
        IEnumerable<Dictionary<string, object>> TaxationVettingApprovalFeedback(int InvoiceMasterID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int ApprovalProcessID);
        Task<TaxationVettingPaymentDto> GetTaxationVettingPayment(int tvpid);

        IEnumerable<Dictionary<string, object>> GetChildList(int TVPID);
        Task<List<PaymentMethodsDetailsDto>> GetPaymentMethodDetails(int TVPID);
    }
}
