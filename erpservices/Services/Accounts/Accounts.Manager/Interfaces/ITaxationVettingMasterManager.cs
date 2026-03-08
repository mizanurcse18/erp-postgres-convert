using Accounts.Manager.Dto;
using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Accounts.Manager
{
    public interface ITaxationVettingMasterManager
    {

        Task<(bool, string)> SaveChanges(TaxationVettingMasterDto list);
        GridModel GetListForGrid(GridParameter parameters);
        GridModel GetListForGridApproved(GridParameter parameters);
        Task<IEnumerable<Dictionary<string, object>>> GetsVDSListAsCombo();
        Task<IEnumerable<Dictionary<string, object>>> GetsTDSListAsCombo(int id);
        Task<Dictionary<string, object>> GetTaxationVettingMaster(int tvmid);
        List<Attachments> GetAttachments(int tvmid);
        List<Attachments> GetAttachmentsInvoice(int InvoiceMasterID); 
         IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> TaxationVettingApprovalFeedback(int InvoiceMasterID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int ApprovalProcessID);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        Task<List<Dictionary<string, object>>> GetInvoiceChildListOfDict(int InvoiceMasterID);
        Task<List<MaterialReceiveDto>> MaterialReceiveMasterDetailsForReassessmentAndView(int POMasterID, int InvoiceMasterID);
        Task<List<ComboModel>> GetInvoiceChildList(int POMasterID);
        Task<List<SCCMasterDto>> SccDetailsForReassessmentAndView(int POMasterID, int InvoiceMasterID);


    }
}
