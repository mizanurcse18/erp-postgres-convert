using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("PettyCashPaymentMaster"), Serializable]
    public class PettyCashPaymentMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PCPMID { get; set; }
        [Loggable]
        [Required]
        public String ReferenceNo { get; set; }
        [Loggable]
        public String ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public int EmployeeID { get; set; }
        [Loggable]
        [Required]
        public DateTime RequestDate { get; set; }
        [Loggable]
        [Required]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        [Required]
        public decimal GrandTotal { get; set; }
        [Loggable]
        public bool IsSettlement { get; set; }
        [Loggable]
        public DateTime? SettlementDate { get; set; }
        [Loggable]
        public int? SettlementBy { get; set; }
        [Loggable]
        public string SettlementRemarks { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
    }
}
