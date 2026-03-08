using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("EmployeeExitInterview")]
    public class EmployeeExitInterview : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long EEIID { get; set; }
        [Loggable]
        [Required]
        public int EmployeeID { get; set; }
        [Loggable]
        [Required]
        public String InterviewDetails { get; set; }
        [Required]
        [Loggable]
        public int ApprovalStatusID { get; set; }        
        [Loggable]
        [Required]
        public bool IsDraft { get; set; }
    }
}
