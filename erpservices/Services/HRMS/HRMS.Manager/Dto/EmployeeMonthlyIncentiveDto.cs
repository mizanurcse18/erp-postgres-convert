using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class EmployeeMonthlyIncentiveDto
    {
        public string Salary_Month_Year { get; set; }
        public string Disbursement_Date { get; set; }
        public string Employee_ID { get; set; }
        public string Designation { get; set; }
        public string Division { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Joining_Date { get; set; }
        public string Adjusted_KPI_Performance_Score_Out_Of_100 { get; set; }
        public string ESSAU_Rating { get; set; }
        public string Attendance_And_Adherence_Quality_Score { get; set; }
        public decimal? Eligible_Incentive { get; set; }
        public decimal? Total_Earnings { get; set; }
        public decimal? Adjustment { get; set; }
        public decimal? Total_Adjustment { get; set; }
        public decimal? Income_Tax { get; set; }
        public decimal? Total_Deduction { get; set; }
        public decimal? Net_Payable { get; set; }
        public string Amount_In_Words { get; set; }
        public decimal? Wallet_Amount { get; set; }
        public string Validation_Result { get; set; } = string.Empty;
    }
}
