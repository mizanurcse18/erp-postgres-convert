using Accounts.DAL.Entities;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Accounts.Manager.Dto
{
    public class PettyCashFilteredData
    {
        public long PCRMID { get; set; }
        public long PCPMID { get; set; }
        public long PCPCID { get; set; }
        public String ReferenceNo { get; set; }
        public DateTime ClaimDate { get; set; }
        public int EmployeeID { get; set; }
        public int CWID { get; set; }
        public long ClaimID { get; set; }
        public decimal TotalAmount { get; set; }
        public int ClaimTypeID { get; set; }
        public string ClaimType { get; set; }
        public int ClaimStatusID { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeWithDepartment { get; set; }
        public long PCRCID { get; set; }
        public int CWEmployeeID { get; set; }
        public int ApprovalStatusID { get; set; }
        public int RApprovalStatusID { get; set; }

        public string ClaimReferenceNo { get; set; }
        public string ReimburseReferenceNo { get; set; }
        public string PaymentReferenceNo { get; set; }

    } 
}
