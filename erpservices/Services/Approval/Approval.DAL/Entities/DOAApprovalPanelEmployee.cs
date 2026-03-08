using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Approval.DAL.Entities
{
    [Table("DOAApprovalPanelEmployee")]
    public class DOAApprovalPanelEmployee : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long DOAApprovalPanelEmployeeID { get; set; }
        [Required]
        [Loggable]
        public long DOAMasterID { get; set; }
        [Required]
        [Loggable]
        public long AssigneeEmployeeID { get; set; }
        [Loggable]
        [Required]
        public int TypeID { get; set; }
        [Required]
        [Loggable]
        public int APPanelID { get; set; }
        [Required]
        [Loggable]
        public long GroupID { get; set; }
    }
}
