using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("MaterialRequisitionChild"), Serializable]
    public class MaterialRequisitionChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MRCID { get; set; }       
        [Loggable]
        public long ItemID { get; set; }
        [Loggable]
        public string Description { get; set; }
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
        public long MRMasterID { get; set; }
        [Loggable]
        public string Remarks { get; set; }
    }
}
