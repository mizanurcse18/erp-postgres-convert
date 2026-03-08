using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonProfessionalCertificationInfo")]
    public class PersonProfessionalCertificationInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PPCIID { get; set; }
        [Required]
        [Loggable]
        public int PersonID { get; set; }      
        [Required]
        [Loggable]
        public string Certification { get; set; }
        [Required]
        [Loggable]
        public string InstituteName { get; set; }
        [Loggable]
        public string Location { get; set; }
        [Loggable]
        public DateTime StartDate { get; set;}
        [Loggable]
        public DateTime EndDate { get; set; }
    }
}
