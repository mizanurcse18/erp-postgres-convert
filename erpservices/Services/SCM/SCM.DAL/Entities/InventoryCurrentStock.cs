using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("InventoryCurrentStock"), Serializable]
    public class InventoryCurrentStock : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ICSID { get; set; }
        [Loggable]
        [Required]
        public long ItemID { get; set; }
        [Loggable]
        [Required]
        public long CurrentStockQty { get; set; }
    }
}
