using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("PettyCashAdvanceMaster"), Serializable]
    public class PettyCashAdvanceMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PCAMID { get; set; }
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
        [Required]
        public long CWID { get; set; }
        [Loggable]
        public int? ClaimStatusID { get; set; }
        [Loggable]
        public bool? IsDisbursement { get; set; }
        [Loggable]
        public int? DisbursementBy { get; set; }
        [Loggable]
        public DateTime? DisbursementDate { get; set; }
        [Loggable]
        public string DisbursementRemarks { get; set; }
        [Loggable]
        public bool? IsResubmit { get; set; }
        [Loggable]
        public int? ResubmitBy { get; set; }
        [Loggable]
        public DateTime? ResubmitDate { get; set; }
        [Loggable]
        public decimal? ReSubmitTotalAmount { get; set; }
        [Loggable]
        public int? ResubmitApprovalStatusID { get; set; }
        public decimal? PayableAmount { get; set; }
        [Loggable]
        public decimal? ReceiveableAmount { get; set; }
        [Loggable]
        public bool? IsSettled { get; set; }
        [Loggable]
        public int? SettledBy { get; set; }
        [Loggable]
        public DateTime? SettledDate { get; set; }
        [Loggable]
        public string SettlementRemarks { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
        [Loggable]
        public bool? IsResubmitDisbursement { get; set; }
        [Loggable]
        public int? ResubmitDisbursementBy { get; set; }
        [Loggable]
        public DateTime? ResubmitDisbursementDate { get; set; }
        [Loggable]
        public string ResubmitDisbursementRemarks { get; set; }
    }
}
