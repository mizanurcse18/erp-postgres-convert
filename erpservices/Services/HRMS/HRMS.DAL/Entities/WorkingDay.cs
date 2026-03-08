using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("WorkingDay")]
    public class WorkingDay : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WorkingDayID { get; set; }
        [Required]
        [Loggable]
        public int FinancialYearID { get; set; }
        [Required]
        [Loggable]
        public int PeriodID { get; set; }
        [Required]
        [Loggable]
        public int WorkingDayCategoryID { get; set; }
        [Required]
        [Loggable]
        public int Year { get; set; }
        [Required]
        [Loggable]
        public int Month { get; set; }
        [Required]
        [Loggable]
        public int Day { get; set; }        
        [Loggable]
        public string Remarks { get; set; }
    }
}
