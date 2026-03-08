using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.DAL.Entities
{
    [Table("CostCategory"), Serializable]
    public class CostCategory : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long CostCategoryID { get; set; }
        [Loggable]
        [Required]
        public string CategoryName { get; set; }
        [Loggable]
        public string ExternalID { get; set; }
    }
}
