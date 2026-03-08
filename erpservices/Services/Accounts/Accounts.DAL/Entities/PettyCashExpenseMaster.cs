using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("PettyCashExpenseMaster"), Serializable]
    public class PettyCashExpenseMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PCEMID { get; set; }
        [Loggable]
        [Required]
        public int EmployeeID { get; set; }        
        [Loggable]
        [Required]
        public String ReferenceNo { get; set; }        
        [Loggable]
        public String ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public DateTime SubmitDate { get; set; }
        [Loggable]
        [Required]
        public long CWID { get; set; }
        [Loggable]
        [Required]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        [Required]
        public decimal GrandTotal { get; set; }
        [Loggable]
        public String Remarks { get; set; }
        [Loggable]
        public int PaymentStatus { get; set; }
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
        public bool? IsSettled { get; set; }
        [Loggable]
        public int? SettledBy { get; set; }
        [Loggable]
        public DateTime? SettlementDate { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
    }
}
