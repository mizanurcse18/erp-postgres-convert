using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SCM.DAL.Entities
{
    [Table("InvoiceChildSCC"), Serializable]
    public class InvoiceChildSCC : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long InvoiceChildSCCID { get; set; }
        [Loggable]
        [Required]
        public long InvoiceMasterID { get; set; }
        [Loggable]
        [Required]
        public long SCCMID { get; set; }
    }
}
