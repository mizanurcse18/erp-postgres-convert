using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("ExpenseClaimChild"), Serializable]
    public class ExpenseClaimChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ECChildID { get; set; }
        [Loggable]
        [Required]
        public long ECMasterID { get; set; }
        [Loggable]
        [Required]
        public DateTime ExpenseClaimDate { get; set; }
        [Loggable]
        [Required]
        public int PurposeID { get; set; }
        [Loggable]
        [Required]
        public string Details { get; set; }
        [Loggable]
        [Required]
        public decimal ExpenseClaimAmount { get; set; }
        [Loggable]        
        public string Remarks { get; set; }

    }
}
