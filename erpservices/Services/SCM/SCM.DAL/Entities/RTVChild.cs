using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("RTVChild"), Serializable]
    public class RTVChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RTVCID { get; set; }
        [Loggable]
        [Required]
        public long RTVMID { get; set; }
        [Loggable]
        [Required]
        public long ItemID { get; set; }
        [Loggable]
        [Required]
        public int POCID { get; set; }
        [Loggable]
        [Required]
        public Decimal SuppliedQty { get; set; }
        [Loggable]
        [Required]
        public Decimal ReturnQty { get; set; }
        [Loggable]
        public string RTVCNote { get; set; }
    }
}
