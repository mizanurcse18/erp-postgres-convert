using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("EmployeeAccessDeactivation")]
    public class EmployeeAccessDeactivation : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long EADID { get; set; }
        [Loggable]
        [Required]
        public int EEIID { get; set; }
        [Loggable]
        [Required]
        public int EmployeeID { get; set; }
        [Loggable]
        [Required]
        public DateTime DateOfResignation { get; set; }
        [Loggable]
        [Required]
        public DateTime LastWorkingDay { get; set; }
        [Loggable]
        public bool? IsCoreFunctional { get; set; }
        [Required]
        [Loggable]
        public int ApprovalStatusID { get; set; }
        [Required]
        [Loggable]
        public bool IsSentForDivisionClearance { get; set; }
        [Required]
        [Loggable]
        public int DivisionClearanceApprovalStatusID { get; set; }
        [Loggable]
        public DateTime? SentForDivisionClearanceDate { get; set; }
        [Loggable]
        [Required]
        public bool IsDraft { get; set; } 
        [Loggable]
        [Required]
        public bool IsDraftForDivClearence { get; set; }
        [Loggable]
        [Required]
        public string Description { get; set; }
    }
}
