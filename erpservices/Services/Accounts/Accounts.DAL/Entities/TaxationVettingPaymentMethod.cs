using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("TaxationVettingPaymentMethod"), Serializable]
    public class TaxationVettingPaymentMethod : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PaymentMethodID { get; set; }
        [Loggable]
        [Required]
        public long TVPID { get; set; }
        [Loggable]
        [Required]
        public int CategoryID { get; set; }
        [Loggable]
        public int FromOrTo { get; set; }
        [Loggable]
        [Required]
        public int BankID { get; set; }
        [Loggable]
        public string VendorBankName { get; set; }
        [Loggable]
        public string BranchName { get; set; }
        [Loggable]
        public string AccountNo { get; set; }
        [Loggable]
        public string RoutingNo { get; set; }
        [Loggable]
        public string SwiftCode { get; set; }
        [Loggable]
        public int? ChequeBookID { get; set; }
        [Loggable]
        public int LeafNo { get; set; }
        [Loggable]
        public decimal Amount { get; set; }
    }
}
