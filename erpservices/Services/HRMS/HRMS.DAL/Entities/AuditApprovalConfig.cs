using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("AuditApprovalConfig")]
    public class AuditApprovalConfig : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MapID { get; set; }
        [Required]
        [Loggable]
        public string QuestionIDs { get; set; }
        [Required]
        [Loggable]
        public string DepartmentIDs { get; set; }
        [Required]
        [Loggable]
        public string DepartmentEmails { get; set; }
        [Loggable]
        public int? ExternalID { get; set; }
        [Loggable]
        public string ExternalProperties { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public bool IsActive { get; set; }

        [Loggable]
        public bool IsRequired { get; set; }

        [Loggable]
        public bool IsPOSMRequired { get; set; }
    }
}
