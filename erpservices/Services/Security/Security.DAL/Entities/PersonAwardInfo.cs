using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PersonAwardInfo")]
    public class PersonAwardInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PAIID { get; set; }
        [Required]
        [Loggable]
        public int PersonID { get; set; }
        [Required]
        [Loggable]
        public string AwardType { get; set; }
        [Required]
        [Loggable]
        public string InstituteName { get; set; }
        [Required]
        [Loggable]
        public int Year { get; set; }
        [Loggable]
        public string Reason { get; set; }
    }
}
