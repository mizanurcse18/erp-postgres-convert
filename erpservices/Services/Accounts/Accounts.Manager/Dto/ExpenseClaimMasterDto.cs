using Accounts.DAL.Entities;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Accounts.Manager
{
    [AutoMap(typeof(ExpenseClaimMaster)), Serializable]
    public class ExpenseClaimMasterDto : Auditable
    {
        public long ECMasterID { get; set; }
    
        public int EmployeeID { get; set; }        
       
        public String ReferenceNo { get; set; }        
     
        public String ReferenceKeyword { get; set; }                     
        public int ApprovalStatusID { get; set; }
      
        public long? IOUMasterID { get; set; }
  
        public decimal GrandTotal { get; set; }
        public decimal PayableAmount { get; set; }
        public DateTime ClaimSubmitDate { get; set; }
        public int PaymentStatus { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string ApprovalStatus { get; set; }
        public string ImagePath { get; set; }
        public string EmployeeCode { get; set; }
        public string ClaimToPaymentTime { get; set; }
        public string PendingAt { get; set; }
        public string EmployeeName { get; set; }
        public int ApprovalProcessID { get; set; }
        public bool IsCurrentAPEmployee { get; set; }
        public int APEmployeeFeedbackID { get; set; }
        public int APForwardInfoID { get; set; }
        public bool IsEditable { get; set; }
        public bool IsReassessment { get; set; }
        public bool IsReturned { get; set; }
        public bool IsDraft { get; set; }
        public bool IsOnBehalf { get; set; }
        public string DivisionName { get; set; }
        public string IOUReferenceNo { get; set; }
        public string EmployeeDetails { get; set; }
        public string Designation { get; set; }
        public string NagadWallet { get; set; }
        public string OnbehalfWallet { get; set; }
    }
}
