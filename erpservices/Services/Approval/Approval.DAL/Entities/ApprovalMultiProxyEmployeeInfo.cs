using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Table("ApprovalMultiProxyEmployeeInfo")]
    public class ApprovalMultiProxyEmployeeInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long AMPEIID { get; set; }
        [Required]
        [Loggable]
        public int APEmployeeFeedbackID { get; set; }
        [Required]
        [Loggable]
        public int ApprovalProcessID { get; set; }        
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
