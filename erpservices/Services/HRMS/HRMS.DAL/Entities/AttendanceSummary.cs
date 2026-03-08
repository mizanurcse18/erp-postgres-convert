using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("AttendanceSummary")]
    public class AttendanceSummary : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PrimaryID { get; set; }
        [Loggable]
        [Required]
        public string EmployeeCode { get; set; }        
        [Loggable]        
        public int EmployeeID { get; set; }
        [Loggable]
        [Required]
        public DateTime AttendanceDate { get; set; }
        [Loggable]        
        public DateTime? InTime { get; set; }
        [Loggable]        
        public DateTime? OutTime { get; set; }
        [Loggable]
        public int TotalTimeInMin { get; set; } = 0;
        [Loggable]
        public int LateInMin { get; set; } = 0;
        [Loggable]
        public int OverTimeInMin { get; set; } = 0;
        [Loggable]
        [Required]
        public int AttendanceStatus { get; set; } = 0;
        [Loggable]
        public string CardNo { get; set; }
        [Loggable]
        [Required]
        public int ShiftID { get; set; }
        [Loggable]        
        public int? LeaveCategoryID { get; set; }
        [Loggable]
        public bool IsEdited { get; set; } = false;
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public string HRNote { get; set; }
        [Loggable]
        public int? ApprovalStatusID { get; set; } = 23;
        [Loggable]
        public string DayStatus { get; set; }
        [Loggable]
        public int? ActualWorkingHourInMin { get; set; }
    }
}
