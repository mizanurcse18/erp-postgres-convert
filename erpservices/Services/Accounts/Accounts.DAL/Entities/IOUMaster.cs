using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("IOUMaster"), Serializable]
    public class IOUMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long IOUMasterID { get; set; }
        [Loggable]
        [Required]
        public int EmployeeID { get; set; }
        [Loggable]
        [Required]
        public DateTime RequestDate { get; set; }
        [Loggable]
        [Required]
        public DateTime SettlementDate { get; set; }
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
    }
}
