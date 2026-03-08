using DAL.Core.Attribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class EmployeePaySlipDto
    {
        public string Salary_Month_Year { get; set; }
        public string Disbursement_Date { get; set; }
        public string Employee_ID { get; set; }
        public string Designation { get; set; }
        public string Division { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Joining_Date { get; set; }
        public decimal? Basic_Salary { get; set; }
        public decimal? House_Rent_Allowance { get; set; }
        public decimal? Medical_Allowance { get; set; }
        public decimal? Conveyance_Allowance { get; set; } = null;
        public decimal? Free_or_Concessional_Passage_for_Travel { get; set; }
        public decimal? Payroll_Card_Part { get; set; }
        public decimal? Arrear_Basic_Salary { get; set; }
        public decimal? Arrear_House_Rent_Allowance { get; set; }
        public decimal? Arrear_Medical_Allowance { get; set; }
        public decimal? Arrear_Conveyance_Allowance { get; set; }
        public decimal? Arrear_Free_Or_Concessional_Passage_For_Travel { get; set; }
        public decimal? Income_Tax { get; set; }
        public decimal? Deduction_Field_1 { get; set; }
        public decimal? Deduction_Field_2 { get; set; }
        public decimal? Total_Earnings { get; set; }
        public decimal? Total_Arrears { get; set; }
        public decimal? Total_Deductions { get; set; }
        public decimal? Net_Payable { get; set; }
        public string Amount_In_Words { get; set; }
        public decimal? Bank_Amount_BDT { get; set; }
        public decimal? Wallet_Amount { get; set; }
        public decimal? Cash_out_Charge { get; set; }
        public decimal? Extra_Mobile_Bill_Deducted { get; set; }
        public decimal? Market_Bonus { get; set; }
        public decimal? Weekend_Allowance { get; set; }
        public decimal? Festival_Holiday_Allowance { get; set; }
        public decimal? Saturday_Allowance { get; set; }
        public decimal? Tax_Support { get; set; }
        public decimal? Festival_Bonus_Arrear { get; set; }
        public decimal? Salary_Advance { get; set; }
        public decimal? Tax_Refund { get; set; }
        public decimal? Laptop_Repairing_Cost_Deducted { get; set; }
        public decimal? Provident_Fund { get; set; }

        public decimal? Mobile_Bill_Adjustment { get; set; }
        public decimal? Car_Allowance { get; set; }

        public string? Validation_Result { get; set; } = null;
    }


}
