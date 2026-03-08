using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class EmployeeMonthlyIncentivePayslipDto
    {
        public long EMIIID { get; set; }
        
        
        public string IncentiveMonth { get; set; }
        
        
        public DateTime DisbursementDate { get; set; }
        public DateTime JoiningDate { get; set; }


        public int EmployeeID { get; set; }
        
        
        public string EmployeeCode { get; set; }
        
        
        public string Designation { get; set; }
        
        
        public string Division { get; set; }
        
        
        public string EmployeeName { get; set; }
        
        
        public string AdjustedKPIPerformanceScore { get; set; }
        
        
        public string ESSAURating { get; set; }
        
        
        public string AttendanceAdherenceScore { get; set; }
        
        
        public string EligibleIncentive { get; set; }
        
        
        public string TotalEarnings { get; set; }
        
        
        public string Adjustment { get; set; }
        
        
        public string TotalAdjustment { get; set; }
        
        
        public string IncomeTax { get; set; }
        
        
        public string TotalDeduction { get; set; }
        
        
        public string NetPayment { get; set; }
        
        
        public string AmountInWords { get; set; }
        
        
        public string WalletAmount { get; set; }
        
        
        public long PATID { get; set; }

        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankBranchName { get; set; }
        public string BankName { get; set; }
        public string WalletNumber { get; set; }
        public string Email { get; set; }
    }
}
