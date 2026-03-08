using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("InvoicePaymentMethod"), Serializable]
    public class InvoicePaymentMethod : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PaymentMethodID { get; set; }
        [Loggable]
        [Required]
        public long IPaymentMasterID { get; set; }
        [Loggable]
        [Required]
        public int CategoryID { get; set; }
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
        public long? SupplierID { get; set; }
        [Loggable]
        public int LeafNo { get; set; }
        [Loggable]
        public decimal NetPayableAmount { get; set; }
    }
}
