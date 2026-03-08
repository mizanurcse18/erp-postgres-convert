using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ApprovalForwardInfo")]
    public class ApprovalForwardInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APForwardInfoID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeFeedbackID { get; set; }
        [Required]
        [Loggable]
        public int ApprovalProcessID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeFeedbackRemarksID { get; set; }
        [Required]
        [Loggable]
        public int DivisionID { get; set; }
        [Required]
        [Loggable]
        public int DepartmentID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Loggable]
        public DateTime? CommentRequestDate { get; set; } 
        [Loggable]
        public DateTime? CommentSubmitDate { get; set; }
        [Loggable]
        public string APEmployeeComment{ get; set; }
        [Loggable]
        public string APForwardEmployeeComment { get; set; }
    }
}
