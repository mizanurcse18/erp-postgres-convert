using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;

namespace Security.DAL.Entities
{
    [Table("Users")]
    public class User : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserID { get; set; }
        [Required]
        [Loggable]
        public string UserName { get; set; }     
        [Required]
        [Loggable]
        public byte[] PasswordHash { get; set; }
        [Required]
        [Loggable]
        public byte[] PasswordSalt { get; set; }         
        [Loggable]
        public int? DefaultApplicationID { get; set; }
        [Required]
        [Loggable]
        public string CompanyID { get; set; }
        [Loggable]
        public bool IsAdmin { get; set; }
        [Loggable]
        public bool IsActive { get; set; }
        [Loggable]
        public DateTime? InActiveDate { get; set; }
        [Loggable]
        public int? AccessFailedCount { get; set; }
        [Loggable]
        public bool TwoFactorEnabled { get; set; }
        [Loggable]
        public bool PhoneNumberConfirmed { get; set; }
        [Loggable]
        public bool EmailConfirmed { get; set; }
        [Loggable]
        public int? PersonID { get; set; }
        [Loggable]
        public bool IsForcedLogin { get; set; }
        [Loggable]
        public string ForgotPasswordToken { get; set; }
        [Loggable]
        public DateTime? TokenValidityTime { get; set; }
        [Loggable]
        public int? DefaultMenuID { get; set; }
        [Loggable]
        public DateTime? ChangePasswordDatetime { get; set; }
        [Loggable]
        public bool IsLocked { get; set; }
        [Loggable]
        public DateTime? LockedDateTime { get; set; }
        [Loggable]
        public int? ReasonID { get; set; }
    }
}