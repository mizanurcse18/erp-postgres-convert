using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("VehicleDetails")]
    public class VehicleDetails : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VehicleID { get; set; }
        [Required]
        [Loggable]
        public string VehicleRegNo { get; set; } 
        [Loggable]
        public string Details { get; set; }
        [Loggable]
        public bool? IsActive { get; set; }
    }
}
