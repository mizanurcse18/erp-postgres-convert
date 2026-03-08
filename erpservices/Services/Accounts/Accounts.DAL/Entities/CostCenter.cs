using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("CostCenter"), Serializable]
    public class CostCenter : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long CostCenterID { get; set; }
        [Loggable]
        [Required]
        public string CostCenterName { get; set; }
        [Loggable]
        [Required]
        public long CostCategoryID { get; set; }
        [Loggable]
        public string ExternalID { get; set; }
    }
}
