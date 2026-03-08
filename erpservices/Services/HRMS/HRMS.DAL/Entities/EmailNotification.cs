using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("EmailNotification")]
    public class EmailNotification : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ENID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Required]      
        [Loggable]
        public string EmailBody { get; set; }
        [Required]
        [Loggable]
        public string To { get; set; }
        [Loggable]
        public string CC { get; set; }
        [Loggable]
        public string BCC { get; set; }
        [Loggable]
        public string MailResoponse { get; set; }
        [Required]
        [Loggable]
        public int GroupID { get; set; }
    }
}
