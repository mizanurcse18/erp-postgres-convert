using SCM.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IInvoicePaymentManager
    {
        Task<List<InvoiceFilteredData>> GetFilteredInvoiceList(DateTime FromDate, DateTime ToDate, string SupplierID, int InvoiceMasterID,int IPaymentMasterID,int paymentTypeId);
        Task<List<InvoiceFilteredData>> GetFilteredInvoicePaymentList(int SupplierID, int InvoiceMasterID,int IPaymentMasterID,int TVPID);
        Task<InvoicePaymentMasterDto> GetMaster(int PaymentMasterID);
        Task<List<InvoiceFilteredData>> GetChildList(int PaymentMasterID);
        Task<List<PaymentMethodsDetailsDto>> GetPaymentMethodDetails(int PaymentMasterID);
        List<InvoicePaymentMasterDto> GetMasterList(string filterData);
        Task<(bool, string)> SaveChanges(InvoicePaymentDto expense);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID);
        IEnumerable<Dictionary<string, object>> ReportApprovalFeedback(int ReferenceID);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        //List<Attachments> GetAttachments(int tvmid);
        #region Grid
        GridModel GetListForGrid(GridParameter parameters);
        #endregion
    }
}
