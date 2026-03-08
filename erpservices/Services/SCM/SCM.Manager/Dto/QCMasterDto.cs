using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using System;

namespace SCM.Manager
{
    [AutoMap(typeof(QCMaster)), Serializable]
    public class QCMasterDto:Auditable
    {
        public long QCMID { get; set; }
        public long RTVMID { get; set; }
        public long MRID { get; set; }
        public string ReferenceNo { get; set; }
        
        public string ReferenceKeyword { get; set; }
        
        public DateTime ReceiptDate { get; set; }
        
        public long WarehouseID { set; get; }
        
        public long SupplierID { get; set; }
        
        public long PRMasterID { get; set; }
        
        public long POMasterID { get; set; }
        
        public decimal TotalSuppliedQty { get; set; }
        
        public decimal TotalAcceptedQty { get; set; }
        
        public decimal TotalRejectedQty { get; set; }
        
        public int ApprovalStatusID { get; set; }
        
        public string BudgetPlanRemarks { get; set; }
        
        public string ChalanNo { get; set; }
        
        public DateTime ChalanDate { get; set; }
        
        public bool IsDraft { get; set; }

        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string AmountInWords { get; set; }
        public string ImagePath { get; set; }
        public int EmployeeID { get; set; }
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
        public decimal GrandTotal { get; set; }
        public bool IsAdvanceInvoice { set; get; }
        public string GRNNo { get; set; }
        public string CreatedDateStr { get { return CreatedDate.ToString("dd MMM yyyy hh:mm tt"); } }
    }
}
