using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(AnnualLeaveEncashmentPolicySettings)), Serializable]
    public class AnnualLeaveEncashmentPolicySettingsDto : Auditable
    {
        public int ALEWMasterID { get; set; }
        public int FinancialYearID { get; set; }
        public int FinancialYearName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        //public string DivisionIDs { get; set; }
        //public string DepartmentIDs { get; set; }
        //public string EmployeeTypeIDs { get; set; }

        public int ALEPSID { get; set; }
        public List<int> EmployeeTypeList { get; set; }
        public int HierarchyLevel { get; set; }
        public int MaximumJobGrade { get; set; }
        public bool IncludeHR { get; set; }
        public decimal MaxEncashablePercent { get; set; }
        public decimal MaxEncashableDays { get; set; }
        public int EmployeeID { get; set; }
        public List<int> ProxyEmployeeID { get; set; } = new List<int>();
        public List<int> DivisionIDList { get; set; } = new List<int>();
        public List<int> DepartmentIDList { get; set; } = new List<int>();
        public List<int> EmployeeTypeIDList { get; set; } = new List<int>();
        public List<int> EmployeeIDList { get; set; } = new List<int>();
        public string ProxyEmployeeStr { get; set; }
        public string JobGradeName { get; set; }
        public string EmployeeName { get; set; }

        public DateTime? CutOffDate { get; set; }
        public string ProxyEmployeeIDs
        {
            get
            {

                return ProxyEmployeeID != null && ProxyEmployeeID.Count > 0 ? String.Join(",", ProxyEmployeeID) : null;

            }
        }


        public string DivisionIDs
        {
            get
            {

                return DivisionIDList != null && DivisionIDList.Count > 0 ? String.Join(",", DivisionIDList) : null;

            }
        }
        public string DepartmentIDs
        {
            get
            {

                return DepartmentIDList != null && DepartmentIDList.Count > 0 ? String.Join(",", DepartmentIDList) : null;

            }
        }
        public string EmployeeTypeIDs
        {
            get
            {

                return EmployeeTypeIDList != null && EmployeeTypeIDList.Count > 0 ? String.Join(",", EmployeeTypeIDList) : null;

            }
        }

        public List<AnnualLeaveEncashmentSettingsDto> AnnualLeaveEncashmentSettings { get; set; } = new List<AnnualLeaveEncashmentSettingsDto>();
        public List<AnnualLeaveEncashmentWindowChildDto> ChildList { get; set; } = new List<AnnualLeaveEncashmentWindowChildDto>();
    }

    public class AnnualLeaveEncashmentSettingsDto
    {
        public int LeaveCategoryID { get; set; }
        public string ApprovalType { get; set; }
        public int MaximumJobGrade { get; set; }
        public string LavelName { get; set; }

    }
}
