using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ApprovalEmployeeFeedback")]
    public class ApprovalEmployeeFeedback : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APEmployeeFeedbackID { get; set; }
        [Required]
        [Loggable]
        public int ApprovalProcessID { get; set; }
        [Required]
        [Loggable]
        public int DivisionID { get; set; }
        [Required]
        [Loggable]
        public int DepartmentID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Required]
        [Loggable]
        public decimal SequenceNo { get; set; }
        [Required]
        [Loggable]
        public int APFeedbackID { get; set; }
        [Loggable]
        public DateTime? FeedbackRequestDate { get; set; }
        [Loggable]
        public DateTime? FeedbackLastResponseDate { get; set; }
        [Loggable]
        public DateTime? FeedbackSubmitDate { get; set; }
        [Loggable]
        public int? ProxyEmployeeID { get; set; }
        [Loggable]
        public bool IsProxyEmployeeEnabled { get; set; }
        [Loggable]
        public int? NFAApprovalSequenceType { get; set; }
        [Loggable]
        public bool IsEditable { get; set; }
        [Loggable]
        public bool IsAutoApproved { get; set; }
        [Loggable]
        public bool IsSCM { get; set; }
        [Loggable]
        public bool IsMultiProxy { get; set; }
        [Loggable]
        public string Particulars { get; set; }
    }
}
