using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCM.DAL.Entities
{
    [Table("InvoicePaymentChild"), Serializable]
    public class InvoicePaymentChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PaymentChildID { get; set; }
        [Loggable]
        [Required]
        public long IPaymentMasterID { get; set; }
        [Loggable]
        [Required]
        public long TVMID { get; set; }
        [Loggable]
        public long? POMasterID { get; set; }
        [Loggable]
        public long? SupplierID { get; set; }
        [Loggable]
        public long? WarehouseID { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal InvoiceAmount { get; set; }
        [Loggable]
        [Required]
        public DateTime ReceivingDate { get; set; }
        [Loggable]
        [Required]
        public DateTime PostingDate { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal CustomDeduction { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal NetPayableAmount { get; set; }
    }
}
