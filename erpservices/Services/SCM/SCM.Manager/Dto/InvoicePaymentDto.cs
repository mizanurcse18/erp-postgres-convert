using SCM.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCM.Manager.Dto
{
    public class InvoicePaymentDto
    {
        public long IPaymentMasterID { get; set; }
        public string ReferenceKeyword { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal GrandTotal { get; set; }
        public int ApprovalProcessID { get; set; }
        public int PaidBy { get; set; }
        public List<InvoicePaymentDetails> Details { get; set; }
        public List<PaymentMethodsDetailsDto> PaymentDetails { get; set; }
        public bool IsException { set; get; }
        public bool IsDraft { set; get; }
    }


    [AutoMap(typeof(InvoicePaymentChild)), Serializable]
    public class InvoicePaymentDetails : Auditable
    {
        public long PaymentChildID { get; set; }
        public long IPaymentMasterID { get; set; }
        public long TVMID { get; set; }
        public long? POMasterID { get; set; }
        public long? MRID { get; set; }
        public long? SupplierID { get; set; }
        public long? WarehouseID { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal TotalPayableAmount { get; set; }
        public DateTime ReceivingDate { get; set; }
        public DateTime PostingDate { get; set; }
        public decimal CustomDeduction { get; set; }
        public decimal NetPayableAmount { get; set; }
    }

    [AutoMap(typeof(InvoicePaymentMethod)), Serializable]
    public class PaymentMethodsDetailsDto : Auditable
    {
        public long PaymentMethodID { get; set; }
        public long IPaymentMasterID { get; set; }
        public int CategoryID { get; set; }
        public int BankID { get; set; }
        public string VendorBankName { get; set; }
        public string BranchName { get; set; }
        public string AccountNo { get; set; }
        public string RoutingNo { get; set; }
        public string SwiftCode { get; set; }
        public int? ChequeBookID { get; set; }
        public long? SupplierID { get; set; }
        public string SupplierName { get; set; }

        public string CBDetails { get; set; }
        public string BankName { get; set; }
        public long ? value { get; set; }
        public string label { get; set; }
        public List<Attachments> Attachments { get; set; }

        //Supplier Bank Info
        public string SupplierBank { get; set; }
        public string SupplierBankAccName { get; set; }
        public string SupplierAccNumber { get; set; }
        public string SuplierBranch { get; set; }
        public string SupplierBINNo { get; set; }
        public string SupplierRoutingNo { get; set; }
        public string SupplierSwiftCode { get; set; }
        public int LeafNo { get; set; }
        public decimal NetPayableAmount { get; set; }
    }


    [AutoMap(typeof(InvoicePaymentMaster)), Serializable]
    public class InvoicePaymentMasterDto : Auditable
    {
        public long IPaymentMasterID { get; set; }
        public string ReferenceNo { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime PaymentDate { get; set; }
        public int ApprovalStatusID { get; set; }
        public int DepartmentID { get; set; }
        public int PaidBy { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
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
        public bool IsDraft { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public string ReferenceKeyword { get; set; }
        public string CreatedDateString { get { return CreatedDate.ToString("yyyy-MM-dd hh:mm tt"); } }
        public bool IsException { set; get; }
        public string WorkMobile { get; set; }
        public string AmountInWords { get; set; }
    }
}
