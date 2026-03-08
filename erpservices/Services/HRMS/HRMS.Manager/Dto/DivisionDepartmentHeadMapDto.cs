using System;
using System.Collections.Generic;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using HRMS.DAL.Entities;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(DivisionHeadMap)), Serializable]
    public class DivisionDepartmentHeadMapDto : Auditable
    {
        public int DHMapID { get; set; }
        public int DivisionID { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public List<DepartmentChildList> ChildList { get; set; }
        public int EmployeeID { set; get; }
        public string EmployeeName { set; get; }
        public decimal BudgetAmount { set; get; }

    }

    public class DepartmentChildList
    {
        public int DepartmentHMapID { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }

        public string DepartmentNameError { get; set; }
        public string DepartmentCodeError { get; set; }
        public string DivisionNameError { get; set; }
        public int EmployeeID{ get; set; }
        public string EmployeeName { get; set; }
    }



    }
