using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("Thana")]
    public class Thana : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ThanaID { get; set; }
        [Loggable]
        public int? DistrictID { get; set; }
        [Required]
        [Loggable]
        public string ThanaName { get; set; }        
        [Required]
        [Loggable]
        public string BanglaThanaName { get; set; }
    }
}
