using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("TaxationVettingPaymentChild"), Serializable]
    public class TaxationVettingPaymentChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long TVPChildID { get; set; }
        [Loggable]
        [Required]
        public long TVPID { get; set; }
        [Loggable]
        [Required]
        public long IPaymentMasterID { get; set; }
        [Loggable]
        public long? POMasterID { get; set; }
        [Loggable]
        public long? SupplierID { get; set; }
    }
}
