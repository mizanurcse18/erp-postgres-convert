using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;

namespace SCM.Manager
{
    [AutoMap(typeof(InvoiceMaster)), Serializable]
    public class InvoiceMasterDto : Auditable
    {
        public long InvoiceMasterID { get; set; }
        
        
        public string InvoiceNo { get; set; }
        
        
        public DateTime InvoiceDate { get; set; }
        
        public DateTime InvoiceReceiveDate { get; set; }
        
        public DateTime? AccountingDate { get; set; }
        
        public int? InvoiceTypeID { get; set; }
        
        public long? POMasterID { get; set; }
        
        public long? MRID { get; set; }
        
        public int CurrencyID { get; set; }
        
        public bool IsAdvanceInvoice { get; set; }
        
        public string InvoiceDescription { get; set; }
        
        
        public long SupplierID { get; set; }
        
        public string ProjectNumber { get; set; }
        
        public decimal? CurrencyRate { get; set; }
        
        public int ApprovalStatusID { get; set; }
        
        public decimal TotalInvoiceAmount { set; get; }
        public decimal VatAmount { set; get; }
        public decimal InvoiceAmount { set; get; }

        public string ExternalID { set; get; }

        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string AmountInWords { get; set; }
        public string ImagePath { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string POEmployeeCode { get; set; }
        public string POEmployeeName { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public int PODivisionID { get; set; }
        public string PODivisionName { get; set; }
        public string DescriptionImageURL { get; set; }
        public int DeliveryLocation { get; set; }
        public string DeliveryLocationName { get; set; }
        public int CountGRN { get; set; }
        public string WorkMobile { set; get; }
        public string PONo { set; get; }
        public string PORemarks { get; set; }
        public string SupplierName { get; set; }
        public string InventoryTypeName { get; set; }
        public long InventoryTypeID { get; set; }
        public string PaymentTermsName { get; set; }

        public string MushakChalanNo { get; set; }

        public DateTime? MushakChalanDate { get; set; }
    }
}
