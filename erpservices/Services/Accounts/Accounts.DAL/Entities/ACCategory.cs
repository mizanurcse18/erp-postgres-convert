using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("ACCategory"), Serializable]
    public class ACCategory: Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CategoryID { get; set; }
        [Loggable]
        [Required]
        public string CategoryName { get; set; }
        [Loggable]
        [Required]
        public string CategoryShortCode { get; set; }
        [Loggable]
        [Required]
        public int SequenceNo { get; set; }
    }
}
