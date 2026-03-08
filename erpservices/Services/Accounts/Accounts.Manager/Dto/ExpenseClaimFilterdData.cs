using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{

    public class ExpenseClaimFilterdData
    {
        public int ECMasterID { get; set; }
        public int DepartmentID { get; set; }
        public int DivisionID { get; set; }
        public int EmployeeID { get; set; }
        public string ReferenceNo { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string DivisionName { get; set; }

        public decimal Travel { get; set; }
        public decimal Food { get; set; }
        public decimal Entertainment { get; set; }
        public decimal ProductionExpense { get; set; }
        public decimal Stationary { get; set; }
        public decimal Others { get; set; }
        public decimal TransferAllowance { get; set; }


        public decimal FoodAndEntertainment { get; set; }
        public decimal Conveyance { get; set; }
        public decimal Stationeries { get; set; }
        public decimal PettyCashReimbursement { get; set; }
        public decimal ForeignTravel { get; set; }
        public decimal LocalTravel { get; set; }


        public decimal TotalAmount { get; set; }
        public DateTime ClaimSubmitDate { get; set; }
        public string ClaimSubmitDateString { get { return ClaimSubmitDate.ToShortDateString(); } }
        public int IOUMasterID { get; set; }
        public string IOUReferenceNo { get; set; }
        public decimal IOUAmount { get; set; }
        public decimal AccountPayableAmountToEmployee { get; set; }
        public string ImagePath { get; set; }
        public long PaymentChildID { get; internal set; }
        //public long PaymentMasterID { get; internal set; }
        public long PaymentMasterID { get; set; }
        public long IOUOrExpenseClaimID { get; internal set; }
        public long? GLID { get; internal set; }
        public decimal ApprovedAmount { get; internal set; }
        public DateTime ReceivingDate { get; internal set; }
        public DateTime PostingDate { get; internal set; }
        public int? PaymentStatus { get; internal set; }
        public string CompanyID { get; internal set; }
        public int CreatedBy { get; internal set; }
        public DateTime CreatedDate { get; internal set; }
        public string CreatedIP { get; internal set; }
        public int? UpdatedBy { get; internal set; }
        public DateTime? UpdatedDate { get; internal set; }
        public string UpdatedIP { get; internal set; }
        public short RowVersion { get; internal set; }
        public int ClaimApprovalProcessID { get; set; }
        public bool IsPettyCash { get; set; }
    }
}
