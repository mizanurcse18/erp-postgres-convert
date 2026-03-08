using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("UnauthorizedLeaveEmailDate")]
    public class UnauthorizedLeaveEmailDate : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ULEID { get; set; }
        [Required]
        [Loggable]
        public int ENID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Loggable]
        [Required]
        public DateTime AttendanceDate { get; set; }
    }
}
