using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Security.DAL.Entities
{
    [Table("NomineeInformation")]
    public class NomineeInformation : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NIID { get; set; }
        [Loggable]
        [Required]
        public string NomineeName { get; set; }
        [Required]
        public string NomineeAddress { get; set; }
        [Loggable]
        [Required]
        public int PersonID { get; set; }
        [Loggable]
        public string RelationShip { get; set; }
        [Loggable]
        public DateTime DateOfBirth { get; set; }
        [Loggable]
        public decimal Percentage { get; set; }
        [Loggable]
        public string NomineeBehalf { get; set; }
    }
}
