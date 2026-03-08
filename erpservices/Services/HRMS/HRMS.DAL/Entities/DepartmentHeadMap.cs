using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("DepartmentHeadMap")]
    public class DepartmentHeadMap : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentHMapID { get; set; }
        [Required]
        [Loggable]
        public int DepartmentID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
    }
}
