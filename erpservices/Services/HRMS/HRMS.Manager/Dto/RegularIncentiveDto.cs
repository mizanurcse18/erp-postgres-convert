using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class RegularIncentiveDto
    {
        public string Incentive_Type_Year { get; set; }
        public string Disbursement_Date { get; set; }
        public string Employee_ID { get; set; }
        public string Designation { get; set; }
        public string Division { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Joining_Date { get; set; }
        public string Particulars_1 { get; set; }
        public decimal? Basic_Entitlement_1 { get; set; }
        public string Particulars_2 { get; set; }
        public decimal? Basic_Entitlement_2 { get; set; }
        public string Particulars_3 { get; set; }
        public decimal? Basic_Entitlement_3 { get; set; }
        public string Particulars_4 { get; set; }
        public decimal? Basic_Entitlement_4 { get; set; }
        public string Eligible_Bonus_BDT { get; set; }
        public decimal? Eligible_Bonus_Total { get; set; }
        public decimal? Income_Tax { get; set; }
        public decimal? Total_Deduction { get; set; }
        public decimal? Net_Payable { get; set; }
        public string Amount_In_Words { get; set; }
        public decimal? Bank_Amount_BDT { get; set; }
        public string Particulars_5 { get; set; }
        public string Performance_rating_1 { get; set; }
        public string Particulars_6 { get; set; }
        public string Performance_rating_2 { get; set; }
        public string Validation_Result { get; set; } = string.Empty;
    }
}
