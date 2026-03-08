using Core;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("Employment")]
    public class Employment : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EmploymentID { get; set; }
        [Loggable]
        public int EmployeeID { get; set; }
        [Loggable]
        public int? EmployeeTypeID { get; set; }
        [Loggable]
        public int? DesignationID { get; set; }
        [Loggable]
        public int? InternalDesignationID { get; set; }
        [Loggable]
        public int? DepartmentID { get; set; }
        [Loggable]
        public int? DivisionID { get; set; }
        [Loggable]
        public int JobGradeID { get; set; }
        [Loggable]
        public int? BranchID { get; set; }
        [Loggable]
        public int? UnitID { get; set; }
        [Loggable]
        public int? SubUnitID { get; set; }
        [Loggable]
        public DateTime? StartDate { get; set; }
        [Loggable]
        public DateTime? EndDate { get; set; }
        [Loggable]
        public bool IsCurrent { get; set; }
        [Loggable]
        public int? ChangeStatusID { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public string Band { get; set; }
        [Loggable]
        public int? ClusterID { get; set; }
        [Loggable]
        public int? RegionID { get; set; }
        [Loggable]
        public int? ShiftID { get; set; }
    }
}
