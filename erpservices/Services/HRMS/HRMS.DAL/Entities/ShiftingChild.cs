using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("ShiftingChild")]
    public class ShiftingChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ShiftingChildID { get; set; }
        [Required]
        [Loggable]
        public int ShiftingMasterID { get; set; }
        [Required]
        [Loggable]
        public int Day { get; set; }
        [Required]
        [Loggable]
        public TimeSpan StartTime { get; set; }
        [Required]
        [Loggable]
        public TimeSpan EndTime { get; set; }
        [Loggable]
        public bool IsWorkingDay { get; set; }
    }
}
