using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Approval.DAL.Entities
{
    [Table("DOAMaster")]
    public class DOAMaster : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DOAMasterID { get; set; }
        [Required]
        [Loggable]
        public long EmployeeID { get; set; }
        [Required]
        [Loggable]
        public DateTime StartDate { get; set; }
        [Required]
        [Loggable]
        public DateTime EndDate { get; set; }
        [Required]
        [Loggable]
        public int StatusID { get; set; }        
        [Loggable]
        public string Remarks { get; set; }
    }
}
