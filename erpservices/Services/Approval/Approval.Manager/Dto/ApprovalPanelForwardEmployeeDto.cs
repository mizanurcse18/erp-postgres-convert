
using Approval.DAL.Entities;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Manager.Dto
{
    [AutoMap(typeof(ApprovalPanelForwardEmployee)), Serializable]
    public class ApprovalPanelForwardEmployeeDto : Auditable
    {
        public int APPanelForwardEmployeeID { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public int APPanelID { get; set; }
        public string PanelName { get; set; }
        public string ImagePath { get; set; }


    }
}
