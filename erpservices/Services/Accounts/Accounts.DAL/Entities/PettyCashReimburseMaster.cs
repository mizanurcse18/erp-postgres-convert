using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("PettyCashReimburseMaster"), Serializable]
    public class PettyCashReimburseMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PCRMID { get; set; }
        [Loggable]
        [Required]
        public string ReferenceNo { get; set; }
        [Loggable]
        public string ReferenceKeyword { get; set; }
        [Loggable]
        [Required]
        public int ApprovalStatusID { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; }
        [Loggable]
        [Required]
        public DateTime ReimburseDate { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
    }
}
