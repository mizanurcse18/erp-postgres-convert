using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("ShiftingMaster")]
    public class ShiftingMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ShiftingMasterID { get; set; }
        [Required]
        [Loggable]
        public string ShiftingName { get; set; }
        [Loggable]
        public int? FirstDayOfWeek { get; set; }
        [Loggable]
        public int ShiftingSlot { get; set; }
        [Loggable]
        public int BufferTimeInMinute { get; set; } = 0;
        [Loggable]
        public DateTime EffectFrom { get; set; }
        [Loggable]
        public string AssignedDepartments { get; set; }
    }
}
