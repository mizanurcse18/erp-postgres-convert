using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(AnnualLeaveEncashmentWindowChild)), Serializable]
    public class AnnualLeaveEncashmentWindowChildDto : Auditable
    {
		public int ALEChildID { get; set; }
		public int ALEWMasterID { get; set; }
		public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string DivisionName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public bool IsMailSent { get; set; }
    }
}
