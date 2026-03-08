using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("Location")]
    public class Location : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LocationID { get; set; }
        [Required]
        [Loggable]
        public string LocationName { get; set; }
        [Loggable]
        public bool? IsActive { get; set; }

    }
}
