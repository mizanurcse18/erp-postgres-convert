using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("PurchaseOrderChild"), Serializable]
    public class PurchaseOrderChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int POCID { get; set; }       
        [Loggable]
        public long ItemID { get; set; }
        [Loggable]
        public string Description { get; set; }        
        [Loggable]
        public int UOM { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal Qty { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal Rate { get; set; }        
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal Amount { get; set; }
        [Loggable]
        public long VatInfoID { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal VatPercent { get; set; }       
        [Loggable]
        public bool IsRebateable { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal RebatePercentage { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalAmountIncludingVat { get; set; }
        [Loggable]
        [Required]
        public long POMasterID { get; set; }
        [Loggable]
        [Required]
        public int PRCID { get; set; }
        [Loggable]
        public long PRQID { set; get; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal VatAmount { get; set; }
    }
}
