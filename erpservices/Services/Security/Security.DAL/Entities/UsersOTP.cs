using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("UsersOTP")]
    public class UsersOTP : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UOTPID { get; set; }
        [Required]
        [Loggable]
        public string OTPHash { get; set; }
        [Loggable]
        public bool IsExpired { get; set; }
        [Loggable]
        public DateTime? ExpiredDate { get; set; }
        [Loggable]
        public int UserID { get; set; }
        [Loggable]
        public bool IsVerified { get; set; }
        [Loggable]
        public string SMPPResponse { get; set; }
        [Loggable]
        public int Year { get; set; }
        [Loggable]
        public int Month { get; set; }
        [Loggable]
        public string CategoryType { get; set; }
    }
}
