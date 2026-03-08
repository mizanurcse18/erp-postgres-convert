using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("CompanyLeavePolicy")]
    public class CompanyLeavePolicy : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CLPolicyID { get; set; }
        [Required]
        [Loggable]
        public int FinancialYearID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeStatusID { get; set; }
        [Required]
        [Loggable]
        public int LeaveCategoryID { get; set; }
        [Required]
        [Loggable]
        public decimal LeaveInDays { get; set; }
        [Loggable]
        public string Remarks { get; set; }
    }
}
