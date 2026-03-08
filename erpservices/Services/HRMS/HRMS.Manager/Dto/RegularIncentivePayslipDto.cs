using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class RegularIncentivePayslipDto
    {
        public long ERIIID { get; set; }
        
        
        public string IncentiveType { get; set; }
        public string QuarterName { get; set; }
        
        
        public DateTime DisbursementDate { get; set; }
        public DateTime JoiningDate { get; set; }


        public int EmployeeID { get; set; }
        
        
        public string EmployeeCode { get; set; }
        
        
        public string Designation { get; set; }
        
        
        public string Division { get; set; }
        
        
        public string EmployeeName { get; set; }
        
        
        public string Particular1 { get; set; }
        
        
        public string BasicEntitlement1 { get; set; }
        
        
        public string Particular2 { get; set; }
        
        
        public string BasicEntitlement2 { get; set; }
        
        
        public string Particular3 { get; set; }
        
        
        public string BasicEntitlement3 { get; set; }
        
        
        public string Particular4 { get; set; }
        
        
        public string BasicEntitlement4 { get; set; }
        
        
        public string EligibleBonus { get; set; }
        
        
        public string EligibleBonusTotal { get; set; }
        
        
        public string IncomeTax { get; set; }
        
        
        public string TotalDeduction { get; set; }
        
        
        public string NetPayable { get; set; }
        
        
        public string AmountInWords { get; set; }
        
       
        public string BankAmount { get; set; }

        public string Particulars5 { get; set; }

        public string PerformanceRating1 { get; set; }

        public string Particulars6 { get; set; }

        public string PerformanceRating2 { get; set; }

        public long PATID { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankBranchName { get; set; }
        public string BankName { get; set; }
        public string WalletNumber { get; set; }
        public string Email { get; set; }
    }
}
