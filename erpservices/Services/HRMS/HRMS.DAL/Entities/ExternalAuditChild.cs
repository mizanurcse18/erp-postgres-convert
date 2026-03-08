using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("ExternalAuditChild")]
    public class ExternalAuditChild : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EACID { get; set; }
        [Required]
        [Loggable]
        public int EAMID { get; set; }
        [Required]
        [Loggable]
        public int AuditQuestionID { get; set; }
        [Required]
        [Loggable]
        public string QuestionFeedback { get; set; }
        [Required]
        [Loggable]
        public int DepartmentID { get; set; }
        [Loggable]
        public string Requirements { get; set; }
        [Loggable]
        public string POSMIDs { get; set; }

    }
}
