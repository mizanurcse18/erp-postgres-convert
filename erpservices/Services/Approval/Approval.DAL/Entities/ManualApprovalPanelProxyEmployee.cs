using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ManualApprovalPanelProxyEmployee")]
    public class ManualApprovalPanelProxyEmployee : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MAPPanelProxyEmployeeID { get; set; }
        [Required]
        [Loggable]
        public int MAPPanelEmployeeID { get; set; }
        [Required]
        [Loggable]
        public int APPanelID { get; set; }
        [Required]
        [Loggable]
        public int DivisionID { get; set; }
        [Required]
        [Loggable]
        public int DepartmentID { get; set; }        
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }   
    }
}
