using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("BudgetChildWithApprovalPanelMap"), Serializable]
    public class BudgetChildWithApprovalPanelMap : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BCWAPPMapID { get; set; }
        [Loggable]
        [Required]
        public long BudgetChildID { get; set; }
        [Loggable]
        [Required]
        public long APPanelID { get; set; }
    }
}
