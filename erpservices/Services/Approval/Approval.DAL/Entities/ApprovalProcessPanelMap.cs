using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ApprovalProcessPanelMap")]
    public class ApprovalProcessPanelMap : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ApprovalProcessPanelMapID { get; set; }        
        [Required]
        [Loggable]
        public int ApprovalProcessID { get; set; }    
        [Required]
        [Loggable]
        public int APPanelID { get; set; }        
        [Loggable]
        public string Remarks { get; set; }        
    }
}
