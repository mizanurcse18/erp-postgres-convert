using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("InventoryWarehouseCurrentStock"), Serializable]
    public class InventoryWarehouseCurrentStock : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IWCSID { get; set; }
        [Loggable]
        [Required]
        public long ItemID { get; set; }
        [Loggable]
        [Required]
        public long WarehouseID { get; set; }
        [Loggable]
        [Required]
        public long CurrentStockQty { get; set; }
    }
}
