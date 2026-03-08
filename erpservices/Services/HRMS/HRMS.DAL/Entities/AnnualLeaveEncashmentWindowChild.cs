using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("AnnualLeaveEncashmentWindowChild")]
    public class AnnualLeaveEncashmentWindowChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ALEChildID { get; set; }
        [Required]
        [Loggable]
        public long ALEWMasterID { get; set; }        
        [Required]
        [Loggable]
        public long EmployeeID { get; set; }
        [Loggable]
        public bool IsMailSent{ get; set; }
    }
}
