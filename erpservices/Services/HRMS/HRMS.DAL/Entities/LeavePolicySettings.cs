using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    public class LeavePolicySettings : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LPSID { get; set; }
        [Required]
        [Loggable]
        public int LeaveCategoryID { get; set; }
        [Required]
        [Loggable]
        public decimal MinimumDays { get; set; }
        [Required]
        [Loggable]
        public decimal MaximumDays { get; set; }
        [Required]
        [Loggable]
        public int DayType { get; set; }
        [Loggable]
        public bool EmployeeTypeException { get; set; }
        [Loggable]
        public string EmployeeTypes{ get; set; }
        [Loggable]
        public bool TanureException { get; set; }
        [Loggable]
        public int EligibilityInMonths { get; set; }
        [Loggable]
        public bool IsHolidayInclusive { get; set; }
        [Loggable]
        public bool IsCarryForwardable { get; set; }
        [Loggable]
        public int MaximumAccumulationDays { get; set; }
        [Loggable]
        public bool IsAttachemntRequired { get; set; }
        [Loggable]
        public int WillApplicableFrom { get; set; }
        [Loggable]
        public int HierarchyLevel { get; set; }
        [Loggable]
        public int MaximumJobGrade { get; set; }
        [Loggable]
        public bool IncludeHRForLFA { get; set; }
        [Loggable]
        public bool IncludeHRForLeave { get; set; }
        [Loggable]
        public decimal ApplicableToHRForDays { get; set; }
        [Loggable]
        public int EmployeeID { get; set; }
        [Loggable]
        public string ProxyEmployeeIDs { get; set; }
        [Loggable]
        public bool IncludeHRForFestival { get; set; }
    }
}
