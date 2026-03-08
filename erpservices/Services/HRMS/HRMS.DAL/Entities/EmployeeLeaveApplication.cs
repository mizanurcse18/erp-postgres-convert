using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("EmployeeLeaveApplication")]
    public class EmployeeLeaveApplication : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EmployeeLeaveAID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Required]
        [Loggable]
        public DateTime ApplicationDate { get; set; }
        [Required]
        [Loggable]
        public DateTime RequestStartDate { get; set; }
        [Required]
        [Loggable]
        public DateTime RequestEndDate { get; set; }
        [Required]
        [Loggable]
        public Decimal NoOfLeaveDays { get; set; }        
        [Loggable]
        public int? BackupEmployeeID { get; set; }
        [Required]
        [Loggable]
        public int ApprovalStatusID { get; set; }
        [Required]
        [Loggable]
        public int LeaveCategoryID { get; set; }
        [Required]
        [Loggable]
        public int FinancialYearID { get; set; }
        [Required]
        [Loggable]
        public int PeriodID { get; set; }        
        [Loggable]
        public string Purpose { get; set; }
        [Loggable]
        public string LeaveLocation { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public int? ParentEmployeeLeaveAID { get; set; }
        [Loggable]
        public bool IsMultiple { get; set; }        
        [Loggable]
        public DateTime? DateOfJoiningWork { get; set; }
        [Loggable]
        public string ExternalID { get; set; }
        [Loggable]
        public string AdditionalFilter { get; set; }
        [Loggable]
        public int CancellationStatus { get; set; } = 0;
        [Loggable]
        public int? CancelledBy { get; set; } = 0;
    }
}
