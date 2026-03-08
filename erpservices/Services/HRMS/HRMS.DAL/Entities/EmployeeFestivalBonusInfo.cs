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
    [Table("EmployeeFestivalBonusInfo")]
    public class EmployeeFestivalBonusInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EFBIID { get; set; }
        [Loggable]
        [Required]
        public string BonusMonth { get; set; }
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
        public string EmployeeName { get; set; }
        [Loggable]
        [Required]
        public string EarningField1 { get; set; }
        [Loggable]
        [Required]
        public string EarningValue1 { get; set; }
        [Loggable]
        [Required]
        public string TotalEarnings { get; set; }
        [Loggable]
        [Required]
        public string DeductionField1 { get; set; }
        [Loggable]
        [Required]
        public string DeductionValue1 { get; set; }
        [Loggable]
        [Required]
        public string DeductionField2 { get; set; }
        [Loggable]
        [Required]
        public string DeductionValue2 { get; set; }
        [Loggable]
        [Required]
        public string TotalDeductions { get; set; }
        [Loggable]
        [Required]
        public string NetPayment { get; set; }
        [Loggable]
        [Required]
        public string AmountInWords { get; set; }
        [Loggable]
        [Required]
        public string BankAmount { get; set; }
        [Loggable]
        [Required]
        public string WalletAmount { get; set; }
        [Loggable]
        [Required]
        public string CashOutCharge { get; set; }
        [Loggable]
        [Required]
        public long PATID { get; set; }


    }
}
