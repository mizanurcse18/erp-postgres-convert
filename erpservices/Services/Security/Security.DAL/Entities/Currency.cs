using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("Currency")]
    public class Currency : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CurrencyID { get; set; }
        [Required]
        [Loggable]
        public string CurrencyName { get; set; }
        [Required]
        [Loggable]
        public string CurrencyDescription { get; set; }
        [Required]
        [Loggable]
        public decimal CurrencyRate { get; set; }
        [Required]
        [Loggable]
        public decimal CurrencyCode { get; set; } 
    }
}
