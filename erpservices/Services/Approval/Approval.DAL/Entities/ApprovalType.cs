using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ApprovalType")]
    public class ApprovalType : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APTypeID { get; set; }
        [Required]
        [Loggable]
        public string Name { get; set; }
        [Loggable]
        public string Description { get; set; }
    }
}
