using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("TaxationVettingMaster"), Serializable]
    public class TaxationVettingMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long TVMID { get; set; }
        [Loggable]
        [Required]
        public DateTime TVMDate { get; set; }
        [Loggable]
        [Required]
        public long InvoiceMasterID { get; set; }
        [Loggable]
        [Required]
        public long PRMasterID { get; set; }
        [Loggable]
        [Required]
        public long POMasterID { get; set; }
        [Loggable]
        [Required]
        public int VATRebatableID { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal VATRebatablePercent { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal VATRebatableAmount { get; set; }
        [Loggable]
        [Required]
        public long VDSRateID { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal VDSRatePercent { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal VDSAmount{ get; set; }
        [Loggable]
        [Required]
        public int TDSMethodID { get; set; }
        [Loggable]
        [Required]
        public long TDSRateID { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TDSRate { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TDSAmount { get; set; }
        [Loggable]
        [Required]
        public String ReferenceNo { get; set; }
        [Loggable]
        public String ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal GrandTotal { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }

    }
}
