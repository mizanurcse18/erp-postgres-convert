using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(LeavePolicySettings)), Serializable]
    public class LeavePolicySettingsDto : Auditable
    {
        public int LPSID { get; set; }
        public int LeaveCategoryID { get; set; }
        public string LeaveCategoryName { get; set; }
        public decimal MinimumDays { get; set; }
        public decimal MaximumDays { get; set; }
        public int DayType { get; set; } = 1;
        public bool EmployeeTypeException { get; set; }
        public string EmployeeTypes { get; set; }
        public bool TanureException { get; set; }
        public int EligibilityInMonths { get; set; }
        public bool IsHolidayInclusive { get; set; }
        public bool IsCarryForwardable { get; set; }
        public int MaximumAccumulationDays { get; set; }
        public bool IsAttachemntRequired { get; set; }
        public int WillApplicableFrom { get; set; }
        public List<int> EmployeeTypeList { get; set; }
        public int HierarchyLevel { get; set; }
        public int MaximumJobGrade { get; set; }
        public bool IncludeHRForLFA { get; set; }
        public bool IncludeHRForLeave { get; set; }
        public bool IncludeHRForFestival { get; set; }
        public decimal ApplicableToHRForDays { get; set; }
        public int EmployeeID { get; set; }
        public List<int> ProxyEmployeeID { get; set; } = new List<int>();
        public string ProxyEmployeeStr { get; set; }
        public string JobGradeName { get; set; }
        public string EmployeeName { get; set; }
        public string ProxyEmployeeIDs
        {
            get
            {

                return ProxyEmployeeID != null && ProxyEmployeeID.Count > 0 ? String.Join(",", ProxyEmployeeID) : null;

            }
        }
        public List<LeaveApprovalPanelSettingsDto> LeaveApprovalPanelSettings { get; set; } = new List<LeaveApprovalPanelSettingsDto>();
    }

    public class LeaveApprovalPanelSettingsDto
    {
        public int LeaveCategoryID { get; set; }
        public string ApprovalType { get; set; }
        public int MaximumJobGrade { get; set; }
        public string LavelName { get; set; }

    }
}
