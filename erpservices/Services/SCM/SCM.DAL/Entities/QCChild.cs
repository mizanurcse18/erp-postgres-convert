using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("QCChild"), Serializable]
    public class QCChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long QCCID { get; set; }
        [Loggable]
        [Required]
        public long QCMID { get; set; }
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
        public Decimal AcceptedQty { get; set; }
        [Loggable]
        [Required]
        public Decimal RejectedQty { get; set; }
        [Loggable]
        public string QCCNote { get; set; }
    }
}
