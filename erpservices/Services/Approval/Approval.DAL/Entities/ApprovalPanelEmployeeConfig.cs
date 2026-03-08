using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{

    [Table("ApprovalPanelEmployeeConfig")]
    public class ApprovalPanelEmployeeConfig : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPEConfigID { get; set; }        
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Required]
        [Loggable]
        public int APPanelID { get; set; }
        [Required]
        [Loggable]
        public decimal SequenceNo { get; set; }
        [Loggable]
        public int? ProxyEmployeeID { get; set; }
        [Loggable]
        public bool IsProxyEmployeeEnabled { get; set; }
        [Loggable]
        public int? NFAApprovalSequenceType { get; set; }
        [Loggable]
        public bool IsEditable { get; set; }    
        [Loggable]
        public bool IsMultiProxy { get; set; }        
        [Loggable]
        public int? TemplateID { get; set; }
    }
}
