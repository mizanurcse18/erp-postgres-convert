using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("ChartOfAccounts"), Serializable]
    public class ChartOfAccounts : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long COAID { get; set; }
        [Loggable]
        [Required]
        public String AccountName { get; set; }
        [Loggable]
        public String AccountCode { get; set; }
        [Loggable]
        public long? ParentID { get; set; }
        [Loggable]
        public int ACClassID { get; set; }
        [Loggable]
        public int CategoryID { get; set; }
        [Loggable]
        public String HierarchyLevel { get; set; }
        [Loggable]
        public int Level { get; set; }
        [Required]
        public int SequenceNo { get; set; }
        [Loggable]
        public decimal? TotalAmount { get; set; }
        [Loggable]
        public decimal? OpeningBalance { get; set; }
        [Loggable]
        public DateTime? OpeningBalanceOn { get; set; }
        [Loggable]
        public string ExternalID { get; set; }
        [Loggable]
        public string BalanceType { get; set; }
        [Loggable]
        public bool IsBudgetEnable { get; set; }
        [Loggable]
        public bool IsActive { get; set; }
        [Loggable]
        public bool IsAllowManualPosting { get; set; }
        [Loggable]
        public DateTime? StartDate { get; set; }
        
    }
}
