using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("Department")]
    public class Department : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DepartmentID { get; set; }
        [Required]
        [Loggable]
        public string DepartmentName { get; set; }
        [Loggable]
        public string DepartmentCode { get; set; }
        [Loggable]
        public int DivisionID { get; set; }
    }
}
