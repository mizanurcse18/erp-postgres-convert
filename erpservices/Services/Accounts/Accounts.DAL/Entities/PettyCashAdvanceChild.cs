using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("PettyCashAdvanceChild"), Serializable]
    public class PettyCashAdvanceChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PCACID { get; set; }
        [Loggable]
        [Required]
        public long PCAMID { get; set; }
        [Loggable]
        [Required]
        public string Details { get; set; }
        [Loggable]
        [Required]
        public string ProjectName { get; set; }
        [Loggable]
        [Required]
        public decimal AdvanceAmount { get; set; }
        [Loggable]        
        public string Remarks { get; set; }
        [Loggable]
        public decimal? ResubmitAmount { get; set; }
        [Loggable]
        public string ResubmitRemarks { get; set; }
    }
}
