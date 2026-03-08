using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;

namespace SCM.Manager
{
    [AutoMap(typeof(PurchaseOrderMaster)), Serializable]
    public class PurchaseOrderMasterDto : Auditable
    {
        public long POMasterID { get; set; }


        public string ReferenceNo { get; set; }

        public string ReferenceKeyword { get; set; }
        public DateTime PODate { get; set; }

        public long? DeliveryLocation { set; get; }
        public string DveliveryLocationName { get; set; }
        public long SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string SupplierAddress { get; set; }
        public string SupplierContact { get; set; }
        public string Warehouse { get; set; }


        public long PRMasterID { get; set; }

        public decimal GrandTotal { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal TotalWithoutVatAmount { get; set; }

        public int ApprovalStatusID { get; set; }
        public DateTime DeliveryWithinDate { get; set; }

        public string ContactPerson { get; set; }

        public string ContactNumber { get; set; }

        public string Remarks { get; set; }
        public string AmountInWords { get; set; }
        public string PORemarks { get; set; }
        public string CloseRemarks { get; set; }


        public string QuotationNo { get; set; }

        public DateTime? QuotationDate { get; set; }

        public int? PaymentTermsID { get; set; }
        public string PaymentTermsName { get; set; }

        public int? InventoryTypeID { get; set; }
        public string InventoryTypeName { get; set; }

        public bool IsDraft { get; set; }

        public string BudgetPlanRemarks { get; set; }

        public string SCMRemarks { get; set; }

        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string ImagePath { get; set; }
        public string EmployeeCode { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int ApprovalProcessID { get; set; }
        public int PRApprovalProcessID { get; set; }

        public bool IsCurrentAPEmployee { get; set; }
        public bool IsClosed { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public int TemplateID { get; set; }
        public string DescriptionImageURL { get; set; }
        public string DeliveryLocationName { get; set; }
        public bool IsSingleQuotation { get; set; }
        public bool IsSCM { get; set; }
        public string PRReferenceNo { get; set; }
        public DateTime PRDate { get; set; }
        public string PREmployeeName { get; set; }
        public string WorkMobile { set; get; }
        public DateTime? LastActionDate { set; get; }
        public string CreatedDateStr { get { return CreatedDate.ToString("dd MMM yyyy hh:mm tt"); } }
        public string PRNo { get; set; }

        // Property For Invoice Creation
        public decimal PreviousBasePercent { get; set; }
        public decimal PreviousBaseAmount { get; set; }
        public decimal PreviousTaxAmount { get; set; }
        public decimal PreviousPaidAmount { get; set; }
        public decimal BasePercent { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal? TaxPercent { get; set; }
        public decimal TotalPayableAmount { get; set; }
        public int NumberOfDueDay { get; set; }

        public decimal PreviousAdvanceBasePercent { get; set; }
        public decimal PreviousAdvanceBaseAmount { get; set; }
        public decimal PreviousAdvanceTaxAmount { get; set; }
        public decimal PreviousAdvanceTotalPayableAmount { get; set; }
        public decimal PreviousAdvanceDeductionAmount { get; set; }
        public decimal AdvanceDeductionAmount { get; set; }
        public string PRBudgetPlanRemarks { get; set; }

        public int CreditDay { get; set; }
    }
}
