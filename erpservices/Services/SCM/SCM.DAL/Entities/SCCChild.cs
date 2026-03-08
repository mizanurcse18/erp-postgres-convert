using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("SCCChild"), Serializable]
    public class SCCChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SCCCID { get; set; }
        [Loggable]
        [Required]
        public long SCCMID { get; set; }
        [Loggable]
        [Required]
        public long ItemID { get; set; }
        [Loggable]
        [Required]
        public int POCID { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal? ReceivedQty { get; set; }
        [Loggable]
        public DateTime? DeliveryOrJobCompletionDate { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal InvoiceAmount { get; set; }
        [Loggable]
        public string SCCCNote { get; set; }

        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal Rate { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal TotalAmount { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmountIncludingVat { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal VatAmount { get; set; }
        [Loggable]
        public string Remarks { get; set; }
    }
}
