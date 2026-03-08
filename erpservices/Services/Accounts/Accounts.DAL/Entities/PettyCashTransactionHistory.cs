using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("PettyCashTransactionHistory"), Serializable]
    public class PettyCashTransactionHistory : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionID { get; set; }
        [Loggable]
        [Required]
        public string ReferenceNo { get; set; }
        [Loggable]
        [Required]
        public int TypeID { get; set; }
        [Loggable]
        [Required]
        public string TypeName { get; set; }
        [Loggable]
        [Required]
        public int MasterID { get; set; }
        [Loggable]
        [Required]
        public decimal PayableAmount { get; set; }
        [Loggable]
        [Required]
        public decimal ReceivableAmount { get; set; }
        [Loggable]
        [Required]
        public int CustodianID { get; set; }

    }
}
