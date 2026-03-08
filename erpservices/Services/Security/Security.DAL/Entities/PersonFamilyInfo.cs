using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonFamilyInfo")]
    public class PersonFamilyInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PFIID { get; set; }
        [Loggable]
        [Required]
        public int PersonID { get; set; }
        [Loggable]
        [Required]
        public int RelationshipTypeID { get; set; }
        [Loggable]
        public string Name { get; set; }
        [Loggable]        
        public int? GenderID { get; set; }
        [Loggable]
        public DateTime? DateOfBirth { get; set; }
    }
}
