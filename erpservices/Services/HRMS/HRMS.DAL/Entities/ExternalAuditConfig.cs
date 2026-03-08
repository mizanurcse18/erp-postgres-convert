using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("ExternalAuditConfig")]
    public class ExternalAuditConfig : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EACID { get; set; }
        [Required]
        [Loggable]
        public int NumberOfUddokta { get; set; }
        [Required]
        [Loggable]
        public int NumberOfMerchant { get; set; }
        [Loggable]
        public DateTime? EffectiveDate { get; set; }
        [Required]
        [Loggable]
        public int NumberOfDays { get; set; }
        [Loggable]
        public bool IsActive { get; set; }
    }
}
