using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("PettyCashPaymentChild"), Serializable]
    public class PettyCashPaymentChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PCPCID { get; set; }
        [Loggable]
        [Required]
        public long PCPMID { get; set; }
        [Loggable]
        [Required]
        public long PCRMID { get; set; }

    }
}
