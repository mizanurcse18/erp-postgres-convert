using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("Item"), Serializable]
    public class Item : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ItemID { get; set; }
        [Loggable]
        [Required]
        public string ItemName { get; set; }
        [Loggable]        
        public string ItemDescription { get; set; }        
        [Loggable]
        public string ItemCodePrefix { get; set; }
        [Loggable]
        public string ItemCodeSuffix { get; set; }
        [Loggable]
        public string ItemCode { get; set; }
        [Loggable]        
        public long? ItemSubGroupID { get; set; }
        [Loggable]
        public int? AssetTypeID { get; set; }
        [Loggable]
        public int? InventoryTypeID { get; set; }
        [Loggable]
        public long? GLID { get; set; }
        [Loggable]
        public int? UnitID { set; get; }
        [Loggable]
        public string ItemNature { get; set; }
        [Loggable]
        public decimal? Price { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public string ExternalID { set; get; }
    }
}
