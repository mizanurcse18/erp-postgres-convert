using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("InvoiceChild"), Serializable]
    public class InvoiceChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long InvoiceChildID { get; set; }
        [Loggable]
        [Required]
        public long InvoiceMasterID { get; set; }
        [Loggable]
        [Required]
        public long MRID{ get; set; }
    }
}
