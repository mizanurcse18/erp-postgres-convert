using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("AnnualLeaveEncashmentMaster")]
    public class AnnualLeaveEncashmentMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ALEMasterID { get; set; }
        [Required]
        [Loggable]
        public long ALEWMasterID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Required]
        [Loggable]
        public decimal TotalLeaveBalanceDays { get; set; }
        [Required]
        [Loggable]
        public decimal EncashedLeaveDays { get; set; }
        [Required]
        [Loggable]
        public int ApprovalStatusID { get; set; }
    }
}
