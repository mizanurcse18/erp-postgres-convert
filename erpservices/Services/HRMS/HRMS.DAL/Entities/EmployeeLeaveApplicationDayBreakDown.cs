using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("EmployeeLeaveApplicationDayBreakDown")]
    public class EmployeeLeaveApplicationDayBreakDown : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ELADBDID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeLeaveAID { get; set; }
        [Required]
        [Loggable]
        public DateTime RequestDate { get; set; }        
        [Required]
        [Loggable]
        public Decimal NoOfLeaveDays { get; set; }
        [Required]
        [Loggable]
        public string HalfOrFullDay { get; set; }
        [Loggable]
        public string AdditionalFilter { get; set; }
        [Loggable]
        public string ExternalID { get; set; }
        [Loggable]
        public bool IsCancelled { get; set; }
    }
}
