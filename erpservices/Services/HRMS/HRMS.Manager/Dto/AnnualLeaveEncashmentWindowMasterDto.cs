using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(AnnualLeaveEncashmentWindowMaster)), Serializable]
    public class AnnualLeaveEncashmentWindowMasterDto : Auditable
    {
		public long ALEWMasterID { get; set; }
		public int FinancialYearID { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Status { get; set; }
		public string DivisionIDs { get; set; }
		public string DepartmentIDs { get; set; }
		public string EmployeeTypeIDs { get; set; }
		public string DivisionIDsStr { get; set; }
		public string DepartmentIDsStr { get; set; }
		public string EmployeeTypeIDsStr { get; set; }
        public string Year { get; set; }
        public string StatusName { get; set; }
    }
}
