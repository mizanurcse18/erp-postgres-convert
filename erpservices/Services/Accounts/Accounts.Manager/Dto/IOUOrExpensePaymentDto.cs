using Accounts.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    public class IOUOrExpensePaymentDto
    {
        public long PaymentMasterID { get; set; }
        public string ReferenceKeyword { get; set; }
        public string SettlementRefNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime SettlementDate { get; set; }
       // public TimeSpan FromTimeLocal { get { return SettlementDate.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(SettlementDate.Value, TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time")).TimeOfDay : DateTime.Now.TimeOfDay; } }

        public decimal GrandTotal { get; set; }
        public int ApprovalProcessID { get; set; }
        public List<ExpenseClaimFilterdData> Details { get; set; }
        public bool IsException { set; get; }

    }


    [AutoMap(typeof(IOUOrExpensePaymentChild)), Serializable]
    public class IOUOrExpensePaymentDetails : Auditable
    {
        public long PaymentChildID { get; set; }

        public long PaymentMasterID { get; set; }

        public long EmployeeID { get; set; }

        public long DepartmentID { get; set; }

        public long IOUOrExpenseClaimID { get; set; }

        public long? GLID { get; set; }

        public decimal ApprovedAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime ReceivingDate { get; set; }

        public DateTime PostingDate { get; set; }
        public int? PaymentStatus { get; set; }
    }


    [AutoMap(typeof(IOUOrExpensePaymentMaster)), Serializable]
    public class IOUOrExpensePaymentMasterDto : Auditable
    {
        public long PaymentMasterID { get; set; }
        public string ReferenceNo { get; set; }
        public string ClaimType { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime PaymentDate { get; set; }

        public int ApprovalStatusID { get; set; }
        public int DepartmentID { get; set; }
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
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public string ReferenceKeyword { get; set; }
        public string PendingAt { get; set; }
        public string CreatedDateString { get { return CreatedDate.ToString("yyyy-MM-dd hh:mm tt"); } }
        public bool IsException { set; get; }
        public bool IsSettlement { set; get; }
        public int? SettledBy { get; set; }        
        public DateTime SettlementDate { get; set; }
        public string SettlementDateString { get { return SettlementDate.ToString("yyyy-MM-dd hh:mm tt"); } }
        public string SettlementRefNo { get; set; }
    }
}
