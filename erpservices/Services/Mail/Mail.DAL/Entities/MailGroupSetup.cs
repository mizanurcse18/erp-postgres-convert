using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mail.DAL.Entities
{
    [Table("MailGroupSetup"), Serializable]
    public class MailGroupSetup : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GroupId { get; set; }
        [Required]
        [Loggable]
        public string GroupName { get; set; }
        [Required]
        [Loggable]
        public int ConfigId { get; set; }        
        [Loggable]
        public string AttachmentPath { get; set; }
        [Required]
        [Loggable]
        public string Subject { get; set; }
        [Required]
        [Loggable]
        public string Body { get; set; }
        [Required]
        [Loggable]
        public int Priority { get; set; }
        [Required]
        [Loggable]
        public int Sensitivity { get; set; }
        [Loggable]
        public DateTime? ReportGenTime { get; set; }
        [Loggable]
        public DateTime? MailGenTime { get; set; }
        [Loggable]
        public string IntervalOn { get; set; }
        [Loggable]
        public decimal IntervalValue { get; set; }
        [Loggable]
        public bool IsFromInterface { get; set; }
    }
}
