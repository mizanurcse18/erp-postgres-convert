using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("InventoryTransaction"), Serializable]
    public class InventoryTransaction : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ITID { get; set; }
        [Loggable]
        [Required]
        public long WarehouseID { set; get; }        
        [Loggable]
        public long? PRMasterID { get; set; }
        [Loggable]
        public long? POMasterID { get; set; }
        [Loggable]
        public long? MRID { get; set; }
        [Loggable]
        public long? TransferID { get; set; }
        [Loggable]
        public long? StoreRequestID { get; set; }
        [Loggable]
        [Required]
        public long ItemID { get; set; }
        [Loggable]
        public long? BatchNo { get; set; }
        [Loggable]
        [Required]
        public decimal TransactionQty { get; set; }
        [Loggable]
        [Required]
        public decimal ItemRate { get; set; }
        [Loggable]
        [Required]
        public string InOrOut { get; set; }
        [Loggable]
        public bool IsTransfer { get; set; }
    }
}
