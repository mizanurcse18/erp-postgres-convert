using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("Holiday")]
    public class Holiday : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int HolidayID { get; set; }
        [Required]
        [Loggable]
        public int FinancialYearID { get; set; }
        [Required]
        [Loggable]
        public string Name { get; set; }
        [Required]
        [Loggable]
        public DateTime HolidayDate { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public string ImagePath { get; set; }
        [Loggable]
        public bool IsFestivalHoliday { get; set; }
    }
}
