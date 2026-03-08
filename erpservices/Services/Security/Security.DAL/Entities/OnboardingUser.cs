using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;

namespace Security.DAL.Entities
{
    [Table("OnboardingUser"), Serializable]
    public class OnboardingUser : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserID { get; set; }        
        [Required]
        [Loggable]
        public string UserName { get; set; }
        [Required]
        [Loggable]
        public string FullName { get; set; }
        [Required]
        [Loggable]
        public string Email { get; set; }
        [Required]
        [Loggable]
        public byte[] PasswordHash { get; set; }
        [Required]
        [Loggable]
        public byte[] PasswordSalt { get; set; }       
        [Loggable]
        public bool IsActive { get; set; }
        [Loggable]
        public bool IsSubmit { get; set; }
        public int? PersonID { get; set; }
        [Loggable]
        public bool IsForcedLogin { get; set; }
        [Loggable]
        public string Division { get; set; }
        [Loggable]
        public string Department { get; set; }
        [Loggable]
        public string Designation { get; set; }
        [Loggable]
        public string MobileNo { get; set; }
        [Loggable]
        public string Location { get; set; }
    }
}