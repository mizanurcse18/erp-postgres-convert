using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;

namespace SCM.Manager
{
    [AutoMap(typeof(PurchaseRequisitionMaster)), Serializable]
    public class PurchaseRequisitionMasterDto:Auditable
    {
        public int PRMasterID { get; set; }
       
        public DateTime? PRDate { get; set; }
       
        public string ReferenceNo { get; set; }
       
        public string Subject { get; set; }
       
        public string Preamble { get; set; }
        public string Description { get; set; }

        public bool IsDraft { get; set; }
        public string PriceAndCommercial { get; set; }
       
        public string Solicitation { get; set; }
       
        public string BudgetPlanRemarks { get; set; }
        public int BudgetPlanCategoryID { get; set; }
        public string BudgetPlanCategoryName { get; set; }

        public decimal GrandTotal { get; set; }
       
        public int ApprovalStatusID { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string AmountInWords { get; set; }
        public string ImagePath { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public int TemplateID { get; set; }
        public string DescriptionImageURL { get; set; }
        public string ReferenceKeyword { get; set; }
        public int DeliveryLocation { get; set; }
        public string SCMRemarks { get; set; }
        public string DeliveryLocationName { get; set; }
        public bool IsSingleQuotation { get; set; }
        public bool IsSCM { get; set; }
        public int CountPO { get; set; }
        public string WorkMobile { set; get; }
        public DateTime? RequiredByDate { get; set; }
        public DateTime? LastActionDate { set; get; }
        public string PODetails { set; get; }
        public decimal TotalQuotedAmount { get; set; }
        public long MRMasterID { get; set; }
        public string MRReferenceNo { get; set; }
        public int MRApprovalProcessID { get; set; }
        public int NFAApprovalProcessID { get; set; }
        public bool IsArchive { get; set; }
        public int NFAID { get; set; }
        public decimal NFAAmount { get; set; }
        public decimal CreatedNFAAmount { get; set; }
        public decimal Balance { get; set; }
        public string NFANo { get; set; }
        public bool IsFromSystem { get; set; }
        public string NFAReferenceNo { get; set; }
        public long PRNFAMapID { get; set; }
    }
}
