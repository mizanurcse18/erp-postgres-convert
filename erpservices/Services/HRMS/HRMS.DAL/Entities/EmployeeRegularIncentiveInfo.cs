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
    [Table("EmployeeRegularIncentiveInfo")]
    public class EmployeeRegularIncentiveInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ERIIID { get; set; }
        [Loggable]
        [Required]
        public string IncentiveType { get; set; }
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
        public string Particular1 { get; set; }
        [Loggable]
        [Required]
        public string BasicEntitlement1 { get; set; }
        [Loggable]
        [Required]
        public string Particular2 { get; set; }
        [Loggable]
        [Required]
        public string BasicEntitlement2 { get; set; }
        [Loggable]
        [Required]
        public string Particular3 { get; set; }
        [Loggable]
        [Required]
        public string BasicEntitlement3 { get; set; }
        [Loggable]
        [Required]
        public string Particular4 { get; set; }
        [Loggable]
        [Required]
        public string BasicEntitlement4 { get; set; }
        [Loggable]
        [Required]
        public string EligibleBonus { get; set; }
        [Loggable]
        [Required]
        public string EligibleBonusTotal { get; set; }
        [Loggable]
        [Required]
        public string IncomeTax { get; set; }
        [Loggable]
        [Required]
        public string TotalDeduction { get; set; }
        [Loggable]
        [Required]
        public string NetPayable { get; set; }
        [Loggable]
        [Required]
        public string AmountInWords { get; set; }
        [Loggable]
        [Required]
        public string BankAmount { get; set; }

        [Loggable]
        [Required]
        public string Particulars5 { get; set; }
        [Loggable]
        [Required]
        public string PerformanceRating1 { get; set; }
        [Loggable]
        [Required]
        public string Particulars6 { get; set; }
        [Loggable]
        [Required]
        public string PerformanceRating2 { get; set; }
          
        [Loggable]
        [Required]
        public long PATID { get; set; }

    }
}
