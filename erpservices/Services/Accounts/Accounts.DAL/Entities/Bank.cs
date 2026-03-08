using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Accounts.DAL.Entities
{
    [Table("Bank"), Serializable]
    public class Bank : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BankID { get; set; }
        [Loggable]
        [Required]
        public string BankName { get; set; }
        [Loggable]
        public string BankAddress { get; set; }
        [Loggable]
        public string ConcernPersonName { get; set; }
        [Loggable]
        public string ConcernPersonPhoneNumber { get; set; }
        [Loggable]
        public bool IsActive { get; set; }
    }
}
