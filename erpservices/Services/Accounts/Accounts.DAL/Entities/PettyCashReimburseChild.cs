using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("PettyCashReimburseChild"), Serializable]
    public class PettyCashReimburseChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PCRCID { get; set; }
        [Loggable]
        [Required]
        public long PCRMID { get; set; }
        [Loggable]
        [Required]
        public long PCCID { get; set; }
        [Loggable]
        [Required]
        public long ClaimTypeID { get; set; }

    }
}
