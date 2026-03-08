using Core;
using Manager.Core.CommonDto;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IInvoiceManager
    {
        GridModel GetListForGrid(GridParameter parameters);
        GridModel GetAdvanceInvoiceListForGrid(GridParameter parameters);
        GridModel GetRegularInvoiceListForGrid(GridParameter parameters);
        Task<Dictionary<string, object>> GetInvoiceMasterDic(int invoiceMasterID);
        Task<Dictionary<string, object>> GetInvoiceMasterDicForTaxationVetting(int invoiceMasterID);
        Task<List<Dictionary<string, object>>> GetInvoiceChildListOfDict(int InvoiceMasterID);
        Task<(bool, string)> SaveInvoice(InvoiceDto MR);
        GridModel GetCreatedInvoiceListForGrid(GridParameter parameters);
        GridModel GetApprovedInvoiceListForGrid(GridParameter parameters);
        Task<PurchaseOrderMasterDto> GetMasterData(int POMasterID);
        Task<List<PurchaseOrderChildDto>> GetChildDataAdvance(int POMasterID);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> InvoiceApprovalFeedback(int InvoiceMasterID);
        List<Attachments> GetAttachments(int InvoiceMasterID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int ApprovalProcessID);
        Task<List<MaterialReceiveDto>> MaterialReceiveMasterDetailsByPO(int POMasterID);
        Task<List<MaterialReceiveDto>> MaterialReceiveMasterDetailsByInvoiceID(int InvoiceMasterID);
        Task<List<MaterialReceiveDto>> MaterialReceiveMasterDetailsForReassessmentAndView(int POMasterID,int InvoiceMasterID);
        Task<List<ComboModel>> GetInvoiceChildList(int POMasterID);
        GridModel GetRegularInvoiceSccListForGrid(GridParameter parameters);
        Task<List<SCCDto>> SCCReceiveMasterDetailsByPO(int POMasterID);
        Task<List<SCCDto>> SCCMasterDetailsByInvoiceID(int InvoiceMasterID);
        int GetExistSccChild(int InvoiceMasterID);
        Task<List<ComboModel>> GetSccInvoiceChildList(int POMasterID);
    }
}
