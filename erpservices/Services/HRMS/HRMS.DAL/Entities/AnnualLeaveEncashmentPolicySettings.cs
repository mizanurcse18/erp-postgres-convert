using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("AnnualLeaveEncashmentPolicySettings")]
    public class AnnualLeaveEncashmentPolicySettings : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ALEPSID { get; set; }
        [Loggable]
        [Required]
        public int HierarchyLevel { get; set; }
        [Loggable]
        [Required]
        public int MaximumJobGrade { get; set; }
        [Loggable]
        public bool IncludeHR { get; set; }
        [Loggable]
        public int EmployeeID { get; set; }
        [Loggable]
        public string ProxyEmployeeIDs { get; set; }
        [Required]
        [Loggable]
        public decimal MaxEncashablePercent { get; set; }
        [Required]
        [Loggable]
        public decimal MaxEncashableDays { get; set; }
        [Loggable]
        public DateTime? CutOffDate { get; set; }

    }
}
