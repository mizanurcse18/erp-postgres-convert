using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("PurchaseRequisitionQuotation"), Serializable]
    public class PurchaseRequisitionQuotation : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PRQID { get; set; }
        [Loggable]
        [Required]
        public long PRMasterID { get; set; }
        [Loggable]
        public long? SupplierID { get; set; }
        [Loggable]
        public string Description { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? Amount { get; set; }
        [Loggable]
        public int? TaxTypeID { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? QuotedQty { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? QuotedUnitPrice { get; set; }
        [Loggable]
        public long? ItemID { get; set; }
        [Loggable]
        public int PRCID { get; set; }
    }
}
