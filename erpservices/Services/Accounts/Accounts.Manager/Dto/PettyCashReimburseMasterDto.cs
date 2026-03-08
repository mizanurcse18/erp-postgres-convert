using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.Manager
{
    [AutoMap(typeof(PettyCashReimburseMaster)), Serializable]
    public class PettyCashReimburseMasterDto : Auditable
    {
        public long PCRMID { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceKeyword { get; set; }
        public int ApprovalStatusID { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime ReimburseDate { get; set; }
        public bool IsDraft { get; set; }
        public DateTime RequestDate { get; set; }
        public bool IsSettlement { get; set; }
        public bool IsDisbursement { get; set; }
        public DateTime? SettlementDate { get; set; }
        public int PaymentMode { get; set; }
        public bool IsReimbursement { get; set; }
        public DateTime? ReimbursementDate { get; set; }
        public decimal ReimburseAmount { get; set; }
        public decimal ReSubmitTotalAmount { get; set; }


        public bool IsResubmit { get; set; }
        public bool IsResubmitDisbursement { get; set; }
        public int ResubmitDisbursementBy { get; set; }
        public DateTime? ResubmitDisbursementDate { get; set; }
        public string ResubmitDisbursementRemarks { get; set; }
        public int DisbursementBy { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public string DisbursementRemarks { get; set; }
        public string PendingAt { get; set; }
        public string ClaimToPaymentTime { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }

        public int DepartmentID { get; set; }
        public string ImagePath { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string WorkMobile { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public int DivisionID { get; set; }
        public int TemplateID { get; set; }
        public int ClaimStatusID { get; set; }
        public string DescriptionImageURL { get; set; }
        public List<Attachments> Attachments { get; set; }
        public List<PettyCashReimburseChildDto> ItemDetails { get; set; }
    }
}
