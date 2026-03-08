using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCM.DAL.Entities
{
    [Table("InvoicePaymentMaster"), Serializable]
    public class InvoicePaymentMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IPaymentMasterID { get; set; }
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
        [Column(TypeName = "decimal(18,4)")]
        public decimal GrandTotal { get; set; }
        [Loggable]
        [Required]
        public DateTime PaymentDate { get; set; }
        [Loggable]
        public int? PaidBy { get; set; }
        [Loggable]
        public bool IsException { get; set; }
        [Loggable]
        public bool IsDraft { get; set; }
    }
}
