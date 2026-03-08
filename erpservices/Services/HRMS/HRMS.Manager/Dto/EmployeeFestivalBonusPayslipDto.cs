using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class EmployeeFestivalBonusPayslipDto
    {
        public long EFBIID { get; set; }
        
        
        public string BonusMonth { get; set; }
        
        
        public DateTime DisbursementDate { get; set; }
        public DateTime JoiningDate { get; set; }


        public int EmployeeID { get; set; }
        
        
        public string EmployeeCode { get; set; }
        
        
        public string Designation { get; set; }
        
        
        public string EmployeeName { get; set; }
        
        
        public string EarningField1 { get; set; }
        
        
        public string EarningValue1 { get; set; }
        
        
        public string TotalEarnings { get; set; }
        
        
        public string DeductionField1 { get; set; }
        
        
        public string DeductionValue1 { get; set; }
        
        
        public string DeductionField2 { get; set; }
        
        
        public string DeductionValue2 { get; set; }
        
        
        public string TotalDeductions { get; set; }
        
        
        public string NetPayment { get; set; }
        
        
        public string AmountInWords { get; set; }
        
        
        public string BankAmount { get; set; }
        
        
        public string WalletAmount { get; set; }
        
        
        public string CashOutCharge { get; set; }
        
        
        public long PATID { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankBranchName { get; set; }
        public string BankName { get; set; }
        public string WalletNumber { get; set; }
        public string Email { get; set; }
    }
}
