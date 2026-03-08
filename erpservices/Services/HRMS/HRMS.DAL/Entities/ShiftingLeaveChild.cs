using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("ShiftingLeaveChild")]
    public class ShiftingLeaveChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ShiftingLeaveChildID { get; set; }
        [Required]
        [Loggable]
        public int ShiftingMasterID { get; set; }
        [Required]
        [Loggable]
        public int ShiftingLeaveCategoryID { get; set; }
        [Required]
        [Loggable]
        public TimeSpan StartTime { get; set; }
        [Required]
        [Loggable]
        public TimeSpan EndTime { get; set; }
    }
}
