using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("BudgetMaster"), Serializable]
    public class BudgetMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BudgetMasterID { get; set; }
        [Loggable]
        [Required]
        public long DepartmentID { get; set; }                
        [Loggable]
        [Required]
        public decimal MinAmount { get; set; }
        [Loggable]
        [Required]
        public decimal TotalMaxAmount { get; set; }
        [Loggable]
        [Required]
        public decimal AttachmentRequiredAmount { get; set; }
    }
}
