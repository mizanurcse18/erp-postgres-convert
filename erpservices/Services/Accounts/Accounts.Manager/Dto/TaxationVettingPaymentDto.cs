using Accounts.DAL.Entities;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.Manager.Dto
{
    [AutoMap(typeof(TaxationVettingPayment)), Serializable]
    public class TaxationVettingPaymentDto : Auditable
    {
        public long TVPID { get; set; }
        public DateTime TVPDate { get; set; }
        public long TVMID { get; set; }
        public DateTime ServicePeriod { get; set; }
        public long PaymentModeID { get; set; }
        public decimal VDSRatePercent { get; set; }
        public decimal VDSAmount { get; set; }
        public decimal TDSRatePercent { get; set; }
        public decimal TDSAmount { get; set; }
        public decimal AdvanceAdjustAmount { get; set; }
        public decimal CashOutRate { get; set; }
        public decimal CashOutAmount { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal NetPayableAmount { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceKeyword { get; set; }
        public int ApprovalStatusID { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalPayableAmount { get; set; }
        public decimal BaseAmount { get; set; }
        public string Purpose { get; set; }
        public bool IsDraft { get; set; }
        public int ApprovalProcessID { get; set; } = 0;
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string ImagePath { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public bool IsAdvanceInvoice { get; set; }
        public string InvoiceReferenceNo { get; set; }
        public string PaymentMode { get; set; }
        public string PONo { get; set; }
        public string WorkMobile { get; set; }
        public string SupplierName { get; set; }
        public DateTime PODate { get; set; }
        public DateTime PRCreatedDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string PRReferenceNo { get; set; }
        public string TDSMethodName { get; set; }
        public string InvoiceNo { get; set; }
        public int InvoiceMasterID { get; set; }
        public int IsBankTransfer { get; set; }
        public List<TaxationVettingPaymentDetailsDto> Details { get; set; }
        public List<PaymentMethodsDetailsDto> PaymentDetails { get; set; }
        public List<PaymentMethodsDetailsDto> ChequeBookBankDetails { get; set; }
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
    }

    [AutoMap(typeof(TaxationVettingPaymentChild)), Serializable]
    public class TaxationVettingPaymentDetailsDto : Auditable
    {
        public long TVPChildID { get; set; }
        public long TVPID { get; set; }
        public long IPaymentMasterID { get; set; }
        public long? POMasterID { get; set; }
        public long? SupplierID { get; set; }


    }

    [AutoMap(typeof(TaxationVettingPaymentMethod)), Serializable]
    public class PaymentMethodsDetailsDto : Auditable
    {
        public long PaymentMethodID { get; set; }
        public long TVPID { get; set; }
        public int CategoryID { get; set; }
        public int BankID { get; set; }
        public int FromOrTo { get; set; }
        public string VendorBankName { get; set; }
        public string ChequeBookBank { get; set; }
        public string BranchName { get; set; }
        public string AccountNo { get; set; }
        public string RoutingNo { get; set; }
        public string SwiftCode { get; set; }
        public int? ChequeBookID { get; set; }
        public int LeafNo { get; set; }
        public decimal Amount { get; set; }
        public string CBDetails { get; set; }
        public string BankName { get; set; }
        public List<Attachments> Attachments { get; set; }


        public string BankNameTo { get; set; }
        public string VendorBankNameTo { get; set; }
        public string BranchNameTo { get; set; }
        public string AccountNoTo { get; set; }
        public string RoutingNoTo { get; set; }
        public string SwiftCodeTo { get; set; }
        public int? ChequeBookIDTo { get; set; }
        public long PaymentMethodIDTo { get; set; }
    }
}
