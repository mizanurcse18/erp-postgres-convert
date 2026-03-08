using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class EmployeeFestivalBonusDto
    {
        public string Bonus_Month_Year { get; set; }
        public string Disbursement_Date { get; set; }
        public string Employee_ID { get; set; }
        public string Designation { get; set; }
        public string Name { get; set; }
        public string Joining_Date { get; set; }
        public string Earning_Field_1 { get; set; }
        public decimal? Earning_Value_1 { get; set; }
        public decimal? Total_Earnings { get; set; }
        public string Deduction_Field_1 { get; set; }
        public decimal? Deduction_Value_1 { get; set; }
        public string Deduction_Field_2 { get; set; }
        public decimal? Deduction_Value_2 { get; set; }
        public decimal? Total_Deduction { get; set; }
        public decimal? Net_Payment { get; set; }
        public string Amount_In_Words { get; set; }
        public decimal? Bank_Amount_BDT { get; set; }
        public decimal? Wallet_Amount { get; set; }
        public decimal? Cash_out_Charge { get; set; }
        public string Validation_Result { get; set; } = string.Empty;
    }
}
