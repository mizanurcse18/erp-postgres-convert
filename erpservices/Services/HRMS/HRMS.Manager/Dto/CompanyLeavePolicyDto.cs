
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(CompanyLeavePolicy)), Serializable]
    public class CompanyLeavePolicyDto : Auditable
    {
        public int CLPolicyID { get; set; }
        public int FinancialYearID { get; set; }
        public int Year { get; set; }
        public int EmployeeStatusID { get; set; }
        public string EmployeeStatus { get; set; }
        public string EmployeeStatusName { get; set; }
        public int LeaveCategoryID { get; set; }
        public string LeaveCategoryName { get; set; }
        public decimal LeaveInDays { get; set; }
        public string Remarks { get; set; }
        public bool IsExists { get; set; }
        public bool IsRemovable { get; set; }
        public List<CompanyLeavePolicyDto> LeavePolicyList { get; set; }

    }
}
