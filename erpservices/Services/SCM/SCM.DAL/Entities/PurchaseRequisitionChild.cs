using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("PurchaseRequisitionChild"), Serializable]
    public class PurchaseRequisitionChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRCID { get; set; }       
        [Loggable]
        public long ItemID { get; set; }
        [Loggable]
        public string Description { get; set; }
        [Loggable]
        public int? ForID { get; set; }
        [Loggable]
        public int UOM { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal? Qty { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal? Price { get; set; }        
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal Amount { get; set; }
        [Loggable]
        [Required]
        public long PRMasterID { get; set; }
        [Loggable]
        public string Remarks { get; set; }
    }
}
