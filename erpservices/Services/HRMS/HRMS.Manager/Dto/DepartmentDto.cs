
using DAL.Core.EntityBase;
using HRMS.DAL.Entities;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    [AutoMap(typeof(Department)), Serializable]
    public class DepartmentDto : Auditable
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }

        public string DepartmentNameError { get; set; }
        public string DepartmentCodeError { get; set; }
        public string DivisionNameError { get; set; }

    }
}
