using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ApprovalPanelProxyEmployee")]
    public class ApprovalPanelProxyEmployee : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPanelProxyEmployeeID { get; set; }
        [Required]
        [Loggable]
        public int APPanelEmployeeID { get; set; }
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
