using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("ExpenseClaimMaster"), Serializable]
    public class ExpenseClaimMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ECMasterID { get; set; }
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
        public int ApprovalStatusID { get; set; }
        [Loggable]
        public long? IOUMasterID { get; set; }
        [Loggable]
        [Required]
        public decimal GrandTotal { get; set; }
        [Loggable]
        [Required]
        public DateTime ClaimSubmitDate { get; set; }
        [Loggable]
        [Required]
        public int PaymentStatus { get; set; }
        [Loggable]
        public bool? IsOnBehalf { get; set; }
        [Loggable]
        [Required]
        public bool IsDraft { get; set; }
    }
}
