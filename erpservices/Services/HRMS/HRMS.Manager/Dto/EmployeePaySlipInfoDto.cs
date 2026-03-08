using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class EmployeePaySlipInfoDto
    {
        public long EPSIID { get; set; }
        
        
        public string SalaryMonth { get; set; }
        
        
        public DateTime DisbursementDate { get; set; }
        public DateTime JoiningDate { get; set; }
        
        
        public int EmployeeID { get; set; }
        
        
        public string EmployeeCode { get; set; }
        
        
        public string Designation { get; set; }
        
        
        public string Division { get; set; }
        
        
        public string EmployeeName { get; set; }
        
        
        public string Department { get; set; }

        public string BasicSalary { get; set; }
        
        
        public string HouseRent { get; set; }
        
        
        public string MedicalAllowance { get; set; }
        public string MobileAllowance { get; set; }
        
        
        public string ConveyanceAllowance { get; set; }
        
        
        public string PassageForTravel { get; set; }
        
        
        public string PayrollCardPart { get; set; }

        //New added two fields Start
        public string MobileBillAdjustment { get; set; }
        public string CarAllowance { get; set; }
        //End

        public string ArrearBasicSalary { get; set; }
        
        
        public string ArrearHouseRent { get; set; }
        
        
        public string ArrearMedicalAllowance { get; set; }
        
        
        public string ArrearConveyanceAllowance { get; set; }
        
        
        public string ArrearPassageForTravel { get; set; }
        
        
        public string TotalEarnings { get; set; }
        
        
        public string TotalArrears { get; set; }
        
        
        public string IncomeTax { get; set; }
        
        
        public string DeductionField1 { get; set; }
        
        
        public string DeductionField2 { get; set; }
        
        
        public string TotalDeductions { get; set; }
        
        
        public string NetPayable { get; set; }
        
        
        public string AmountInWords { get; set; }
        
        
        public string BankAmount { get; set; }
        
        
        public string WalletAmount { get; set; }
        
        
        public string CashOutCharge { get; set; }


        
        public string MarketBonus { get; set; }

        public string WeekendAllowance { get; set; }

        public string FestivalHolidayAllowance { get; set; }

        public string SaturdayAllowance { get; set; }

        public string TaxSupport { get; set; }

        public string FestivalBonusArrear { get; set; }

        public string SalaryAdvance { get; set; }

        public string TaxRefund { get; set; }

        public string LaptopRepairingCostDeducted { get; set; }

        public string ProvidentFund { get; set; }


        public long PATID { get; set; }


        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankBranchName { get; set; }
        public string BankName { get; set; }
        public string WalletNumber { get; set; }
    }


}
