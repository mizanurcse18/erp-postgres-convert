using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("DriverDetails")]
    public class DriverDetails : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DriverID { get; set; }
        [Required]
        [Loggable]
        public string DriverName { get; set; } 
        [Loggable]
        public string ContactNumber { get; set; }
        [Loggable]
        public bool? IsActive { get; set; }
    }
}
