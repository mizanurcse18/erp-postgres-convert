using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonEmergencyContactInfo")]
    public class PersonEmergencyContactInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PECIID { get; set; }
        [Required]
        [Loggable]
        public int PersonID { get; set; } 
        [Loggable]
        public string Name { get; set; }
        [Loggable]
        public string ContactNo { get; set; }
        [Loggable]
        public string Relationship { get; set; }
        [Loggable]
        public string Email { get; set; }
        [Loggable]
        public string Address { get; set; }

    }
}
