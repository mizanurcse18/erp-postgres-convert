using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("VoucherCategory"), Serializable]
    public class VoucherCategory : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long VoucherCategoryID { get; set; }
        [Loggable]
        [Required]
        public string CategoryName { get; set; }
        [Loggable]
        public string ExternalID { get; set; }
    }
}
