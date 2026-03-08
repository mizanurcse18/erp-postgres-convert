using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("PettyCashExpenseChild"), Serializable]
    public class PettyCashExpenseChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PCECID { get; set; }
        [Loggable]
        [Required]
        public long PCEMID { get; set; }
        [Loggable]
        [Required]
        public DateTime ExpenseDate { get; set; }
        [Loggable]
        [Required]
        public int PurposeID { get; set; }
        [Loggable]
        [Required]
        public string Details { get; set; }
        [Loggable]
        [Required]
        public decimal ExpenseAmount { get; set; }
        [Loggable]        
        public string Remarks { get; set; }

    }
}
