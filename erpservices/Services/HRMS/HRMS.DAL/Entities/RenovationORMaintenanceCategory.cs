using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.DAL.Entities
{
    [Table("RenovationORMaintenanceCategory")]
    public class RenovationORMaintenanceCategory : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ROMID { get; set; }
        [Required]
        [Loggable]
        public string RenovationName { get; set; }
        [Loggable]
        public decimal SequenceNo { get; set; }
    }
}
