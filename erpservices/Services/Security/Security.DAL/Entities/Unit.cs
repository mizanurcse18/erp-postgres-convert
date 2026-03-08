using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("Unit")]
    public class Unit : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UnitID { get; set; }
        [Loggable]
        [Required]
        public string UnitCode { get; set; }
        [Required]
        [Loggable]
        public string UnitShortCode { get; set; }        
        [Required]
        [Loggable]
        public decimal? LelativeFactor { get; set; }
    }
}
