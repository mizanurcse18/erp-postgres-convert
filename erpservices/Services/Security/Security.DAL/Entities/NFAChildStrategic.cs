using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("NFAChildStrategic")]
    public class NFAChildStrategic : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NFACSID { get; set; }
        [Loggable]
        [Required]
        public long ItemID { get; set; }
        [Loggable]
        public string Description { get; set; }
        [Loggable]
        [Required]
        public int UOM { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal Qty { get; set; }
        [Loggable]
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal UnitPrice { get; set; }
        [Loggable]
        [Column(TypeName = "decimal(18,4)")]
        public Decimal TotalAmount { get; set; }
        [Loggable]
        public int NFAMasterID { get; set; }
    }
}
