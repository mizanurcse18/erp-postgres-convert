using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("MaterialReceiveChild"), Serializable]
    public class MaterialReceiveChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long MRCID { get; set; }        
        [Loggable]
        [Required]
        public long MRID { get; set; }
        [Loggable]
        [Required]
        public long ItemID { get; set; }
        [Loggable]
        [Required]
        public long QCCID { get; set; }
        [Loggable]
        [Required]
        public Decimal ReceiveQty { get; set; }
        [Loggable]
        [Required]
        public Decimal ItemRate { get; set; }
        [Loggable]
        [Required]
        public Decimal TotalAmount { get; set; }
        [Loggable]
        public decimal TotalAmountIncludingVat { get; set; }        
        [Loggable]
        public decimal VatAmount { get; set; }
    }
}
