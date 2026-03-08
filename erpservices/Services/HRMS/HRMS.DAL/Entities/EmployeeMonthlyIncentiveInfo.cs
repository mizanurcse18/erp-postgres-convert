using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("EmployeeMonthlyIncentiveInfo")]
    public class EmployeeMonthlyIncentiveInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EMIIID { get; set; }
        [Loggable]
        [Required]
        public string IncentiveMonth { get; set; }
        [Loggable]
        [Required]
        public DateTime DisbursementDate { get; set; }
        [Loggable]
        [Required]
        public int EmployeeID { get; set; }
        [Loggable]
        [Required]
        public string EmployeeCode { get; set; }
        [Loggable]
        [Required]
        public string Designation { get; set; }
        [Loggable]
        [Required]
        public string Division { get; set; }
        [Loggable]
        [Required]
        public string EmployeeName { get; set; }
        [Loggable]
        [Required]
        public string AdjustedKPIPerformanceScore { get; set; }
        [Loggable]
        [Required]
        public string ESSAURating { get; set; }
        [Loggable]
        [Required]
        public string AttendanceAdherenceScore { get; set; }
        [Loggable]
        [Required]
        public string EligibleIncentive { get; set; }
        [Loggable]
        [Required]
        public string TotalEarnings { get; set; }
        [Loggable]
        [Required]
        public string Adjustment { get; set; }
        [Loggable]
        [Required]
        public string TotalAdjustment { get; set; }
        [Loggable]
        [Required]
        public string IncomeTax { get; set; }
        [Loggable]
        [Required]
        public string TotalDeduction { get; set; }
        [Loggable]
        [Required]
        public string NetPayment { get; set; }
        [Loggable]
        [Required]
        public string AmountInWords { get; set; }
        [Loggable]
        [Required]
        public string WalletAmount { get; set; }
        [Loggable]
        [Required]
        public long PATID { get; set; }

    }
}
