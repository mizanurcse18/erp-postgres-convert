using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Approval.DAL.Entities
{
    [Serializable]
    [Table("DynamicApprovalPanelEmployee")]
    public class DynamicApprovalPanelEmployee : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DAPEID { get; set; }
        [Loggable]
        [Required]
        public string Title { get; set; }
        [Loggable]
        [Required]
        public int HierarchyLevel { get; set; }
        [Loggable]
        [Required]
        public int MaximumJobGrade { get; set; }
        [Required]
        [Loggable]
        public string DivisionIDs { get; set; }
        [Loggable]
        public string DepartmentIDs { get; set; }
        [Loggable]
        public string EmployeeIDs { get; set; }
        [Loggable]
        public bool IncludeHR { get; set; }
        [Loggable]
        public int HREmployeeID { get; set; }
        [Loggable]
        public string HRProxyEmployeeIDs { get; set; }
        [Required]
        [Loggable]
        public decimal MinLimitAmount { get; set; }
        [Required]
        [Loggable]
        public decimal MaxLimitAmount { get; set; }                
        [Loggable]
        public bool IncludeDivisionHead { get; set; }
        [Loggable]
        public bool IncludeDepartmentHead { get; set; }
        [Required]
        [Loggable]
        public string ApprovalPanels { get; set; }
        [Loggable]
        public bool IsActive { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Loggable]
        public string ExternalID { get; set; }
    }
}
