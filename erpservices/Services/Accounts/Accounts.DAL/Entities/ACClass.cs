using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("ACClass"), Serializable]
    public class ACClass: Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACClassID { get; set; }
        [Loggable]
        [Required]
        public string ClassName { get; set; }
        [Loggable]
        [Required]
        public string ShortCode { get; set; }
        [Loggable]        
        public string BalanceType { get; set; }
        [Loggable]
        [Required]
        public int SequenceNo { get; set; }
    }
}
