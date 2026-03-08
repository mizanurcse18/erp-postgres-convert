
using Approval.DAL.Entities;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Approval.Manager.Dto
{
    [AutoMap(typeof(DynamicApprovalPanelEmployee)), Serializable]
    public class DynamicApprovalPanelWindowDto : Auditable
    {
        public int DAPEID { get; set; }
        public string Title { get; set; }
        public int HierarchyLevel { get; set; }
        public int MaximumJobGrade { get; set; }
        //public string DivisionIDs { get; set; }
        //public string DepartmentIDs { get; set; }
        public string EmployeeIDs { get; set; }
        public bool IncludeHR { get; set; }
        public int HREmployeeID { get; set; }
        //public string HRProxyEmployeeIDs { get; set; }
        public decimal MinLimitAmount { get; set; }
        public decimal MaxLimitAmount { get; set; }
        public bool IncludeDivisionHead { get; set; }
        public bool IncludeDepartmentHead { get; set; }
        //public string ApprovalPanels { get; set; }
        public bool IsActive { get; set; }
        public string Remarks { get; set; }
        public string ExternalID { get; set; }

        public string ActiveStatus { get; set; }

        public int EmployeeID { get; set; }
        public List<int> HRProxyEmployeeID { get; set; } = new List<int>();
        public List<int> DivisionIDList { get; set; } = new List<int>();
        public List<int> APPanelIDList { get; set; } = new List<int>();
        public List<int> DepartmentIDList { get; set; } = new List<int>();
        public List<int> EmployeeIDList { get; set; } = new List<int>();
        public string ProxyEmployeeStr { get; set; }
        public string JobGradeName { get; set; }
        public string EmployeeName { get; set; }

        public string DivisionIDsStr { get; set; }
        public string DepartmentIDsStr { get; set; }
        public string ApprovalPanelsIDsStr { get; set; }
        public string HRProxyEmployeeStr { get; set; }
        public string HRProxyEmployeeIDs
        {
            get
            {

                return HRProxyEmployeeID != null && HRProxyEmployeeID.Count > 0 ? String.Join(",", HRProxyEmployeeID) : null;

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
        public string ApprovalPanels
        {
            get
            {

                return APPanelIDList != null && APPanelIDList.Count > 0 ? String.Join(",", APPanelIDList) : null;

            }
        }

    }
}
