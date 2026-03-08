using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCM.DAL.Entities
{
    [Table("VatInfo"), Serializable]
    public class VatInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long VatInfoID { get; set; }
        [Loggable]
        public decimal VatPercent { get; set; }
        [Loggable]
        [Required]
        public string VatPolicies { get; set; }        
        [Loggable]
        public bool IsRebateable { get; set; }
        [Loggable]
        public decimal RebatePercentage { get; set; }
    }
}
