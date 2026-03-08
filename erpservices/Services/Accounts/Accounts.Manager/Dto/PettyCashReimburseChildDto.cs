using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Accounts.Manager
{
    [AutoMap(typeof(PettyCashReimburseChild)), Serializable]
    public class PettyCashReimburseChildDto : Auditable
    {
        public long PCRCID { get; set; }
        public long ClaimID { get; set; }
        public long PCRMID { get; set; }
        public long PCCID { get; set; }
        public long ClaimTypeID { get; set; }
        public string ClaimType { get; set;}
        public decimal TotalAmount { get; set;}
        public string DepartmentName { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DivisionName { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime ClaimDate { get; set; }
        public int ClaimApprovalProcessID { get; set; }
        public long PCAMID { get; set; }
    }
}
