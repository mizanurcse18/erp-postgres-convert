using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("PayrollAuditTrial")]
    public class PayrollAuditTrial : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PATID { get; set; }
        [Loggable]
        [Required]
        public string FileName { get; set; }
        [Loggable]
        [Required]
        public DateTime UploadedDateTime { get; set; }
        [Loggable]
        [Required]
        public int ActivityTypeID { get; set; }
        [Loggable]
        [Required]
        public string ActivityPeriod { get; set; }
        [Loggable]
        [Required]
        public int ActivityStatusID { get; set; }
        [Loggable]
        public int? FestivalBonusTypeID { get; set; }
    }
}
