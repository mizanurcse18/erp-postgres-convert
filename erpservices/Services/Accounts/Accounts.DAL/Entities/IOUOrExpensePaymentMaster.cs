using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("IOUOrExpensePaymentMaster"), Serializable]
    public class IOUOrExpensePaymentMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PaymentMasterID { get; set; }               
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
        public decimal GrandTotal { get; set; }
        [Loggable]
        [Required]
        public DateTime PaymentDate { get; set; }
        [Loggable]
        public int? PaidBy { get; set; }
        [Loggable]
        [Required]
        public string ClaimType { get; set; }
        [Loggable]
        public bool IsException { get; set; }
        [Loggable]
        public bool IsSettlement { get; set; }
        [Loggable]
        public int? SettledBy { get; set; }
        [Loggable]
        public DateTime? SettlementDate { get; set; }
        [Loggable]
        public String SettlementRefNo { get; set; }
    }
}
