using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("PurchaseRequisitionQuotationItemMap"), Serializable]
    public class PurchaseRequisitionQuotationItemMap : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PRQItemMapID { get; set; }
        [Loggable]
        [Required]
        public long PRMasterID { get; set; }
        [Loggable]
        [Required]
        public long PRQID { get; set; }
        [Loggable]
        [Required]
        public long ItemID { get; set; }
    }
}
