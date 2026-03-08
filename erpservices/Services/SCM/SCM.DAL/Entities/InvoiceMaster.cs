using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("InvoiceMaster"), Serializable]
    public class InvoiceMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long InvoiceMasterID { get; set; }
        [Loggable]
        [Required]
        public string InvoiceNo { get; set; }
        [Loggable]
        [Required]
        public DateTime InvoiceDate { get; set; }
        [Loggable]
        public DateTime InvoiceReceiveDate { get; set; }
        [Loggable]
        public DateTime? AccountingDate { get; set; }
        [Loggable]
        public int? InvoiceTypeID { get; set; }
        [Loggable]
        public long? POMasterID { get; set; }
        [Loggable]
        public int CurrencyID { get; set; }
        [Loggable]
        public bool IsAdvanceInvoice { get; set; }
        [Loggable]
        public string InvoiceDescription { get; set; }
        [Required]
        [Loggable]
        public long SupplierID { get; set; }
        [Loggable]
        public string ProjectNumber { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? CurrencyRate { get; set; }
        [Loggable]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? BasePercent { set; get; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal BaseAmount { set; get; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TaxPercent { set; get; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TaxAmount { set; get; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalPayableAmount { set; get; }
        [Loggable]
        public string ExternalID { set; get; }
        [Loggable]
        public string ReferenceNo { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public decimal AdvanceDeductionAmount { get; set; } = 0;
        [Loggable]
        public int? GracePeriod { get; set; }
        [Loggable]
        public string MushakChalanNo { get; set; }
        [Loggable]
        public DateTime? MushakChalanDate { get; set; }

    }
}
