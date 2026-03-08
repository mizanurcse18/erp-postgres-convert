using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Security.DAL.Entities
{
    [Table("UserLogTracker")]
    public class UserLogTracker : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogedID { get; set; }
        public int UserID { get; set; }
        public DateTime LogInDate { get; set; }
        public DateTime? LogOutDate { get; set; }
        public bool? IsLive { get; set; }
        public string IPAddress { get; set; }
        public string CompanyID { get; set; }
        public bool IsLoginFailed { get; set; }
        public int? ReasonID { get; set; }
    }
}