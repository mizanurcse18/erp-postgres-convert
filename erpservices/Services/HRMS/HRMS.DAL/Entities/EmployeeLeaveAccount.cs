using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HRMS.DAL.Entities
{
    [Table("EmployeeLeaveAccount")]
    public class EmployeeLeaveAccount : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ELAID { get; set; }
        [Required]
        [Loggable]
        public int FinancialYearID { get; set; }
        [Required]
        [Loggable]
        public int EmployeeID { get; set; }
        [Required]
        [Loggable]
        public int LeaveCategoryID { get; set; }
        [Required]
        [Loggable]
        public decimal LeaveDays { get; set; }
        [Loggable]
        public string Remarks { get; set; }
        [Required]
        [Loggable]
        public decimal ApprovedDays { get; set; }
        [Required]
        [Loggable]
        public decimal PendingDays { get; set; }
        [Required]
        [Loggable]
        public decimal RemainingDays { get; set; }
        [Required]
        [Loggable]
        public decimal PreviousLeaveDays { get; set; }

        //[Loggable]
        //public decimal? EncashedLeaveDays { get; set; }
    }
}
