using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("IOUChild"), Serializable]
    public class IOUChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IOUChildID { get; set; }
        [Loggable]
        [Required]
        public long IOUMasterID { get; set; }
        [Loggable]
        [Required]
        public string Description { get; set; }
        [Loggable]
        [Required]
        public decimal IOUAmount { get; set; }
        [Loggable]        
        public string Remarks { get; set; }
    }
}
