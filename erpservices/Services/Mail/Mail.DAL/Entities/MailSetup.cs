using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Mail.DAL.Entities
{
    [Table("MailSetup"), Serializable]
    public class MailSetup : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MailId { get; set; }
        [Required]
        [Loggable]
        public int GroupId { get; set; }
        [Required]
        [Loggable]
        public string To_CC_BCC { get; set; }
        [Required]
        [Loggable]
        public string Email { get; set; }
    }
}
