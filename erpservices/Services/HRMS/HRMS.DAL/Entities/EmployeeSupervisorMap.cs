using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("EmployeeSupervisorMap")]
    public class EmployeeSupervisorMap : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MapID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeSupervisorID { get; set; }
        [Loggable]
        public bool IsCurrent { get; set; }

        [Loggable]
        public int SupervisorType { get; set; }

        [Loggable]
        public DateTime? FromDate { get; set; }
        [Loggable]
        public DateTime? ToDate { get; set; }
    }
}
