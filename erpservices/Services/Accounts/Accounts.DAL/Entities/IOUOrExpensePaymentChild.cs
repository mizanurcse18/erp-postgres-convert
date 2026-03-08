using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("IOUOrExpensePaymentChild"), Serializable]
    public class IOUOrExpensePaymentChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PaymentChildID { get; set; }
        [Loggable]
        [Required]
        public long PaymentMasterID { get; set; }
        [Loggable]
        [Required]
        public long EmployeeID { get; set; }
        [Loggable]
        [Required]
        public long DepartmentID { get; set; }      
        [Loggable]
        [Required]
        public long IOUOrExpenseClaimID { get; set; }        
        [Loggable]        
        public long? GLID { get; set; }
        [Loggable]
        [Required]
        public decimal ApprovedAmount { get; set; }               
        [Loggable]
        [Required]
        public decimal TotalAmount { get; set; }        
        [Loggable]
        [Required]
        public DateTime ReceivingDate { get; set; }
        [Loggable]
        [Required]
        public DateTime PostingDate{ get; set; }        
        [Loggable]
        public int? PaymentStatus { get; set; }
    }
}
