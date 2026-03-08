using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("UserTokenBlackList")]
    public class UserTokenBlackList : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UTBID { get; set; }
        [Required]
        [Loggable]
        public int UserID { get; set; }
        [Required]
        [Loggable]
        public string Token { get; set; }
    }
}
