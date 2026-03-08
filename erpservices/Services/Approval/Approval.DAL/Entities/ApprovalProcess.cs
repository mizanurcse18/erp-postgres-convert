using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ApprovalProcess")]
    public class ApprovalProcess : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ApprovalProcessID { get; set; }
        [Required]
        [Loggable]
        public int APTypeID { get; set; }
        [Required]
        [Loggable]
        public int ReferenceID { get; set; }
        [Required]
        [Loggable]
        public string Description { get; set; }
        [Required]
        [Loggable]
        public string Title { get; set; }
        [Required]
        [Loggable]
        public int APStatusID { get; set; }
    }
}
