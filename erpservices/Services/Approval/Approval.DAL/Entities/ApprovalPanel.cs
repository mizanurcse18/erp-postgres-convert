using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ApprovalPanel")]
    public class ApprovalPanel : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPanelID { get; set; }
        
        [Required]
        [Loggable]
        public string Name { get; set; }        
        [Loggable]
        public string Description { get; set; }       
        [Loggable]
        public int? APTypeID { get; set; }
        [Loggable]
        public bool IsParallelApproval { get; set; }
        [Loggable]
        public bool IsDynamicApproval { get; set; }
    }
}
