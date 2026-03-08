using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonAcademicInfo")]
    public class PersonAcademicInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PAIID { get; set; }
        [Required]
        [Loggable]
        public int PersonID { get; set; }
        [Required]
        [Loggable]
        public string DegreeOrCertification { get; set; }
        [Required]
        [Loggable]
        public string InstituteName { get; set; }
        [Loggable]
        public string BoardOrUniversity { get; set; }
        [Required]
        [Loggable]
        public string SubjectOrArea { get; set; }
        [Required]
        [Loggable]
        public int PassingYear { get; set; }
        [Required]
        [Loggable]
        public string Result { get; set; }
        [Required]
        [Loggable]
        public bool IsLastAcademic { get; set; }

    }
}
