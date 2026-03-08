using System;
using System.Collections.Generic;
using System.Text;

namespace Accounts.Manager.Dto
{

    public class IOUClaimFilterdData
    {
        public int PaymentChildID { get; set; }
        public int IOUMasterID { get; set; }
        public int DepartmentID { get; set; }
        public int PaymentMasterID { get; set; }
        public int DivisionID { get; set; }
        public int EmployeeID { get; set; }
        public string ReferenceNo { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public string DivisionName { get; set; }
        public string Description { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal IOUAmount { get; set; }
        public string ClaimRefNo { get; set; }
        public DateTime SettlementDate { get; set; }
        public string SettlementDateString { get { return SettlementDate.ToShortDateString(); } }
        public string ImagePath { get; set; }   
    }
}
