using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("ChequeBookChild"), Serializable]
    public class ChequeBookChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CBCID { get; set; }
        [Loggable]
        [Required]
        public int CBID { get; set; }
        [Loggable]
        [Required]
        public int LeafNo { get; set; }
        [Loggable]
        public bool IsActiveLeaf { get; set; }
        [Loggable]
        public bool IsUsed { get; set; }
    }
}
