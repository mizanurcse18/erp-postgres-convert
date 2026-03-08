using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonReferenceInfo")]
    public class PersonReferenceInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRIID { get; set; }
        [Required]
        [Loggable]
        public int PersonID { get; set; }
        [Loggable]
        [Required]
        public int? ReferenceTypeID { get; set; }
        [Loggable]
        [Required]
        public string ReferenceName { get; set; }
        [Loggable]
        public string Organization { get; set; }
        [Loggable]
        public string Designation { get; set; }
        [Loggable]
        public string Email { get; set; }
        [Loggable]
        public string Mobile { get; set; }
        [Loggable]
        public string Relationship { get; set; }
        [Loggable]
        public bool IsCompanyEmployee { get; set; }

    }
}
