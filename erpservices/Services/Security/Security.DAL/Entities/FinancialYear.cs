using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("FinancialYear")]
    public class FinancialYear : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FinancialYearID { get; set; }
        [Required]
        [Loggable]
        public int Year { get; set; }
        [Loggable]
        public string YearDescription { get; set; }
        [Loggable]
        public bool IsCurrent{ get; set; }
    }
}
