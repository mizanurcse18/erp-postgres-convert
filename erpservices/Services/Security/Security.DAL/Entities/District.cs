using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("District")]
    public class District : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DistrictID { get; set; }
        [Loggable]
        public int? DivisionID { get; set; }
        [Required]
        [Loggable]
        public string DistrictName { get; set; }        
        [Required]
        [Loggable]
        public string BanglaDistrictName { get; set; }
    }
}
