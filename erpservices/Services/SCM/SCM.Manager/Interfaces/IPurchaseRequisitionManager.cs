using Core;
using Manager.Core.CommonDto;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IPurchaseRequisitionManager
    {
        Task<(bool, string)> SaveChanges(PurchaseRequisitionDto PR);
        Task<List<PurchaseRequisitionMasterDto>> GetPurchaseRequisitionList(string filterData);
        Task<PurchaseRequisitionMasterDto> GetPurchaseRequisitionMaster(int PRMasterID, int isSCM);
        Task<PurchaseRequisitionMasterDto> GetApprovedPurchaseRequisitionMaster(int PRMasterID); 
        Task<PurchaseRequisitionMasterDto> GetNFABalanceByPRID(long PRMasterID, int? nfaid); 
         Task<List<PurchaseRequisitionChildDto>> GetPurchaseRequisitionChild(int PRMasterID);
        Task<List<PurchaseRequisitionChildDto>> GetPurchaseRequisitionChildForPO(int PRMasterID);
        Task<List<PurchaseRequisitionChildDto>> GetPurchaseRequisitionForReassessment(int PRMasterID,int POMasterID);
        IEnumerable<Dictionary<string, object>> GetApprovalComment(int aprovalProcessId);

        List<Attachments> GetAttachments(int PRMasterID);
        Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID);
        Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId);
        Task<List<PurchaseRequisitionMasterDto>> GetAllApproved();
        List<QuotationDto> GetQuotations(int PRMasterID);
        List<Attachments> GetAssesments(int PRMasterID);
        bool GetIsAssessmentMember();
        Task<IEnumerable<Dictionary<string, object>>> GetSuppliers(string param);
        IEnumerable<Dictionary<string, object>> ReportForPRApprovalFeedback(int PRID);
        IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID);
        Task<List<PRChildCostCenterBudgetDto>> GetPurchaseRequisitionChildCostCenterBudget(int PRMasterID);
        Task SaveArchiveStatus(int PRMasterID, bool IsArchive);
        #region Grid
        Task<GridModel> GetPRListForGrid(GridParameter parameters);
        Task<GridModel> GetApprovePRListForGrid(GridParameter parameters);
        #endregion
    }
}
