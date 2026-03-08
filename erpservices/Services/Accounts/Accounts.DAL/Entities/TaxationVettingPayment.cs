using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("TaxationVettingPayment"), Serializable]
    public class TaxationVettingPayment : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long TVPID { get; set; }
        [Loggable]
        [Required]
        public DateTime TVPDate { get; set; }
        [Loggable]
        [Required]
        public long TVMID { get; set; }
        [Loggable]
        [Required]
        public DateTime ServicePeriod { get; set; }
        [Loggable]
        [Required]
        public int PaymentModeID { get; set; }
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
        [Column(TypeName = "decimal(18,4)")]
        public decimal TDSRatePercent { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TDSAmount { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal AdvanceAdjustAmount { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal CashOutRate { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal CashOutAmount { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal PayableAmount { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal NetPayableAmount { get; set; }
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
        public string Purpose { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
        [Loggable]
        public string  VatChallanNo { get; set; }
        [Loggable]
        public DateTime? ChallanDate { get; set; }

    }
}
