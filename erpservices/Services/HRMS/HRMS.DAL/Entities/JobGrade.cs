using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("JobGrade")]
    public class JobGrade : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int JobGradeID { get; set; }
        [Required]
        [Loggable]
        public string JobGradeName { get; set; }
        [Loggable]
        public decimal SequenceNo { get; set; }
    }
}
