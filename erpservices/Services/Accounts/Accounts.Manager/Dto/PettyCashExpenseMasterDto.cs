using Accounts.DAL.Entities;
using AutoMapper;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{
    [AutoMap(typeof(PettyCashExpenseMaster)), Serializable]
    public class PettyCashExpenseMasterDto : Auditable
    {
        public long PCEMID { get; set; }
        public int EmployeeID { get; set; }
        public String ReferenceNo { get; set; }
        public String ReferenceKeyword { get; set; }
        public int ApprovalStatusID { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PayableAmount { get; set; }
        public DateTime SubmitDate { get; set; }
        public String Remarks { get; set; }
        public int SettleApprovalStatusID { get; set; }
        public long CWID { get; set; }
        public int PaymentStatus { get; set; }
        public int TransferTypeID { get; set; }
        public string PendingAt { get; set; }
        public string EmployeeName { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsDisbursement { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public bool IsDraft { get; set; }
        public string AmountInWords { get; set; }

        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentID { get; set; }
        public string ImagePath { get; set; }
        public string EmployeeCode { get; set; }


    }
}
