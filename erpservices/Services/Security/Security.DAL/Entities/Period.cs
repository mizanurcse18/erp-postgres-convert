using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("Period")]
    public class Period : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PeriodID { get; set; }
        [Required]
        [Loggable]
        public int FinancialYearID { get; set; }
        [Required]
        [Loggable]
        public DateTime PeriodStartDate { get; set; }
        [Required]
        [Loggable]
        public DateTime PeriodEndtDate { get; set; }
        [Loggable]
        public decimal SeqNo { get; set; }
        [Loggable]
        public bool IsCurrent { get; set; }
    }
}
