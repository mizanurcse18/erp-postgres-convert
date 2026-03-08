using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mail.DAL.Entities
{
    [Table("MailConfiguration"), Serializable]
    public class MailConfiguration : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ConfigId { get; set; }
        [Required]
        [Loggable]
        public string ConfigName { get; set; }
        [Required]
        [Loggable]
        public string Host { get; set; }
        [Required]
        [Loggable]
        public int Port { get; set; }
        [Required]
        [Loggable]
        public string UserName { get; set; }
        [Required]
        [Loggable]
        public string DisplayName { get; set; }
        [Required]
        [Loggable]
        public string Password { get; set; }
        [Required]
        [Loggable]
        public bool IsActive { get; set; }
        [Required]
        [Loggable]
        public decimal SeqNo { get; set; }
        [Required]
        [Loggable]
        public bool EnableSsl { get; set; }
        [Required]
        [Loggable]
        public int Timeout { get; set; }
        [Required]
        [Loggable]
        public int SleepTime { get; set; }
    }
}
