using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("SystemConfiguration")]
    public class SystemConfiguration : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SystemConfigurationID { get; set; }
        [Loggable]
        [Required]
        public int UserAccountLockedDurationInMin { get; set; }
        [Required]
        [Loggable]
        public int UserPasswordChangedDurationInDays { get; set; }
        [Required]
        [Loggable]
        public int AccessFailedCountMax { get; set; }
        [Required]
        [Loggable]
        public bool IsActive { get; set; }
    }
}
