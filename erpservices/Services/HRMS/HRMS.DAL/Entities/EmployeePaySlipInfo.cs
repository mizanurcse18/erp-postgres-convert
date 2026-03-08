using DAL.Core.Attribute;
using DAL.Core.EntityBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Security.DAL.Entities
{
    [Table("EmployeePaySlipInfo")]
    public class EmployeePaySlipInfo : Auditable
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long EPSIID { get; set; }
        [Loggable]
        [Required]
        public string SalaryMonth { get; set; }
        [Required]
        [Loggable]
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
        [Required]
        [Loggable]
        public string Department { get; set; }
        [Required]
        [Loggable]
        public string BasicSalary { get; set; }
        [Required]
        [Loggable]
        public string HouseRent { get; set; }
        [Required]
        [Loggable]
        public string MedicalAllowance { get; set; }
        [Loggable]
        [Required]
        public string ConveyanceAllowance { get; set; }
        [Loggable]
        [Required]
        public string PassageForTravel { get; set; }
        [Loggable]
        [Required]
        public string PayrollCardPart { get; set; }
        [Loggable]
        [Required]
        public string ArrearBasicSalary { get; set; }
        [Loggable]
        [Required]
        public string ArrearHouseRent { get; set; }
        [Loggable]
        [Required]
        public string ArrearMedicalAllowance { get; set; }
        [Loggable]
        [Required]
        public string ArrearConveyanceAllowance { get; set; }
        [Loggable]
        [Required]
        public string ArrearPassageForTravel { get; set; }
        [Loggable]
        [Required]
        public string TotalEarnings { get; set; }
        [Loggable]
        [Required]
        public string TotalArrears { get; set; }
        [Loggable]
        [Required]
        public string IncomeTax { get; set; }
        [Loggable]
        [Required]
        public string DeductionField1 { get; set; }
        [Loggable]
        [Required]
        public string DeductionField2 { get; set; }
        [Loggable]
        [Required]
        public string TotalDeductions { get; set; }
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
        public string WalletAmount { get; set; }
        [Loggable]
        [Required]
        public string CashOutCharge { get; set; }
        [Loggable]
        [Required]
        public long PATID { get; set; }
        [Loggable]
        public string MobileAllowance { get; set; }
        [Loggable]
        public string MarketBonus { get; set; }
        [Loggable]
        public string WeekendAllowance { get; set; }
        [Loggable]
        public string FestivalHolidayAllowance { get; set; }
        [Loggable]
        public string SaturdayAllowance { get; set; }
        [Loggable]
        public string TaxSupport { get; set; }
        [Loggable]
        public string FestivalBonusArrear { get; set; }
        [Loggable]
        public string SalaryAdvance { get; set; }
        [Loggable]
        public string TaxRefund { get; set; }
        [Loggable]
        public string LaptopRepairingCostDeducted { get; set; }
        [Loggable]
        public string ProvidentFund { get; set; }
        [Loggable]
        public string MobileBillAdjustment { get; set; }
        [Loggable]
        public string CarAllowance { get; set; }
    }
}
