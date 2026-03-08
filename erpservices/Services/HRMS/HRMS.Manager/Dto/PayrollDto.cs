using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class PayrollDto
    {
       
    }

    public class PaySlipCSVDto
    {
        public string Salary_Month_Year { get; set; }
        public string Disbursement_Date { get; set; }
        public string Employee_ID { get; set; }
        public string Designation { get; set; }
        public string Division { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Joining_Date { get; set; }
        public string Basic_Salary { get; set; }
        public string House_Rent_Allowance { get; set; }
        public string Medical_Allowance { get; set; }
        public string Conveyance_Allowance { get; set; } = null;
        public string Free_Or_Concessional_Passage_For_Travel { get; set; }
        public string Payroll_Card_Part { get; set; }
        public string Arrear_Basic_Salary { get; set; }
        public string Arrear_House_Rent_Allowance { get; set; }
        public string Arrear_Medical_Allowance { get; set; }
        public string Arrear_Conveyance_Allowance { get; set; }
        public string Arrear_Free_Or_Concessional_Passage_For_Travel { get; set; }
        public string Income_Tax { get; set; }
        public string Deduction_Field_1 { get; set; }
        public string Deduction_Field_2 { get; set; }
        public string Total_Earnings { get; set; }
        public string Total_Arrears { get; set; }
        public string Total_Deductions { get; set; }
        public string Net_Payable { get; set; }
        public string Amount_In_Words { get; set; }
        public string Bank_Amount_BDT { get; set; }
        public string Wallet_Amount { get; set; }
        public string Cash_Out_Charge { get; set; }
        public string Extra_Mobile_Bill_Deducted { get; set; }
        public string Market_Bonus { get; set; }
        public string Weekend_Allowance { get; set; }
        public string Festival_Holiday_Allowance { get; set; }
        public string Saturday_Allowance { get; set; }
        public string Tax_Support { get; set; }
        public string Festival_Bonus_Arrear { get; set; }
        public string Salary_Advance { get; set; }
        public string Tax_Refund { get; set; }
        public string Laptop_Repairing_Cost_Deducted { get; set; }
        public string Provident_Fund { get; set; }
        public string Mobile_Bill_Adjustment { get; internal set; }
        public string Car_Allowance { get; internal set; }
    }
}
