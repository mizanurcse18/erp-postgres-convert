using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("UserWiseUddoktaOrMerchantMapping")]
    public class UserWiseUddoktaOrMerchantMapping : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MAPID { get; set; }

        [Required]
        [Loggable]
        public int EmployeeID { get; set; }

        [Required]
        [Loggable]
        public string WalletNumber { get; set; }
        [Loggable]
        public string WalletName { get; set; }

        [Required]
        [Loggable]
        public int TypeID { get; set; }

        [Loggable]
        public bool IsActive { get; set; }

        [Loggable]
        public bool IsTagged { get; set; }
    }
}
