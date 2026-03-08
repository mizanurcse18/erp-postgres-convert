using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("AuditQuestion")]
    public class AuditQuestion : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuestionID { get; set; }
        [Required]
        [Loggable]
        public string Title { get; set; }
        [Loggable]
        public string Description { get; set; }
        [Loggable]
        public int? ExternalID { get; set; }
        [Loggable]
        public bool IsActive { get; set; }
    }
}
