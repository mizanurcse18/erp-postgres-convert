using Core;
using Manager.Core.CommonDto;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IPurchaseOrderManager
    {

        Task<Dictionary<string, object>> GetSupplierByID(int POMasterID);
        Task<Dictionary<string, object>> GetCompanyInfo();
        Task<IEnumerable<Dictionary<string, object>>> GetTerms();
        Task<(bool, string)> SaveChanges(PurchaseOrderDto PR);
        Task ClosePurchaseOrder(PurchaseOrderDto PO);
        Task UpdatePurchaseOrderMasterAfterReset(PurchaseOrderDto PO); 
         Task<List<PurchaseOrderMasterDto>> GetPurchaseOrderList(string filterData);
        Task<PurchaseOrderMasterDto> GetPurchaseOrderMaster(int POMasterID);
        Task<PurchaseOrderMasterDto> GetPurchaseOrderMasterReassessment(int POMasterID); 
         Task<PurchaseOrderMasterDto> GetPurchaseOrderMasterFromApprovedPR(int POMasterID);
        Task<List<PurchaseOrderChildDto>> GetPurchaseOrderChild(int POMasterID);
        Task<List<PurchaseOrderChildDto>> GetPurchaseOrderChildSCC(int POMasterID);
        Task<List<PurchaseOrderChildDto>> GetPurchaseOrderChildForQC(int POMasterID);
        Task<List<PurchaseOrderChildDto>> GetPurchaseOrderChildForSCC(int POMasterID);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);

        List<Attachments> GetAttachments(int PRMasterID);
        Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        //Task<List<PurchaseOrderMasterDto>> GetAllApproved();
        //Task<IEnumerable<Dictionary<string, object>>> GetsSupplierFromPRQuotation(int PRID); 
        Task RemovePurchaseOrder(int POMasterID, int aprovalProcessId);
        IEnumerable<Dictionary<string, object>> ReportForPOApprovalFeedback(int POID);
        Dictionary<string, object> ReportForPOApprovalFeedbackWithTerm(int POID); 
         IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);

        //Task<List<PurchaseOrderMasterDto>> GetAllApproved(string fromDate, string toDate);
        GridModel GetAllApproved(GridParameter parameters);
        GridModel GetAllApprovedForSCC(GridParameter parameters);

        List<ManualApprovalPanelEmployeeDto> GetQCApprovalPanelDefault(int POMasterID);
        List<ManualApprovalPanelEmployeeDto> GetSCCApprovalPanelDefault(int POMasterID);

        #region Grid
        Task<GridModel> GetPOListForGrid(GridParameter parameters);
        Task<GridModel> GetPOListForSCCGrid(GridParameter parameters);
        #endregion
    }
}
