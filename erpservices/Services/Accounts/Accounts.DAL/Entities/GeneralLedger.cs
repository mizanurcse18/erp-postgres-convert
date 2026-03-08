using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("GeneralLedger"), Serializable]
    public class GeneralLedger : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GLID { get; set; }
        [Loggable]
        [Required]
        public String GLName { get; set; }
        [Loggable]
        public String GLCode { get; set; }
        [Loggable]
        public long? GLGroupID { get; set; }
        [Loggable]
        public decimal? GLTotalAmount { get; set; }
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
        [Loggable]
        public int GLTypeID { get; set; }
        [Loggable]
        public int GLLayerID { get; set; }
    }
}
