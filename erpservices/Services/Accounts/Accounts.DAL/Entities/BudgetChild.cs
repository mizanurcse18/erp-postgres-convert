using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("BudgetChild"), Serializable]
    public class BudgetChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BudgetChildID { get; set; }
        [Loggable]
        [Required]
        public long BudgetMasterID { get; set; }
        [Loggable]
        [Required]
        public decimal MinAmount { get; set; }
        [Loggable]
        [Required]
        public decimal MaxAmount { get; set; }       
    }
}
