using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("Employee")]
    public class Employee : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EmployeeID { get; set; }
        [Loggable]
        public string FullName { get; set; }
        [Loggable]
        public string EmployeeCode { get; set; }
        [Loggable]
        public int PersonID{ get; set; }
        [Loggable]
        public DateTime? DateOfJoining { get; set; }
        [Loggable]
        public string WorkEmail { get; set; }
        [Loggable]
        public string WorkMobile { get; set; }
        [Loggable]
        public int? EmployeeStatusID { get; set; }
        [Loggable]
        public DateTime? DiscontinueDate { get; set; }
        [Loggable]
        public int? EmploymentCategoryID { get; set; }
        [Loggable]
        public DateTime? ConfirmDate { get; set; }
        [Loggable]
        public string WalletNumber { get; set; }
    }
}
