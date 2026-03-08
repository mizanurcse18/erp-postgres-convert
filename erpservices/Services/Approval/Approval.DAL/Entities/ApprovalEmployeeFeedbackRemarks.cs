using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ApprovalEmployeeFeedbackRemarks")]
    public class ApprovalEmployeeFeedbackRemarks : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APEmployeeFeedbackRemarksID { get; set; }        
        [Required]
        [Loggable]
        public int ApprovalProcessID { get; set; } 
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public DateTime? RemarksDateTime { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Required]
        [Loggable]
        public int APFeedbackID { get; set; }        
        [Loggable]
        public int? ProxyEmployeeID { get; set; }
        [Loggable]
        public bool IsProxyEmployeeRemarks { get; set; }
        [Loggable]
        public string ProxyEmployeeRemarks { get; set; }

    }
}
