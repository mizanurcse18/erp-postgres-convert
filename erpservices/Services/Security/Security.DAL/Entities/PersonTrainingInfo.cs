using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonTrainingInfo")]
    public class PersonTrainingInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PTIID { get; set; }
        [Required]
        [Loggable]
        public int PersonID { get; set; }      
        [Required]
        [Loggable]
        public string Title { get; set; }
        [Loggable]
        public string Trainer { get; set; }
        [Required]
        [Loggable]
        public int CountryID { get; set; }
        [Required]
        [Loggable]
        public string InstituteName { get; set; }
        [Required]
        [Loggable]
        public int TrainingYear { get; set; }
        [Loggable]
        public string DurationType { get; set; }
        [Loggable]
        public int? Duration { get; set; }
        [Loggable]
        public string Location { get; set; }

    }
}
