using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Manager.Dto
{
    public class PaySlipModelDto
    {
        public int year { get; set; }
        public int month { get; set; }
        public int FesivalBonusTypeID { get; set; }
        public string monthName { get; set; }
        public string fiscalYear { get; set; }
        public string YearMonthError { get; set; }
        public string APINotFoundError { get; set; }
        public string APIUnreachableError { get; set; }
        public string CategoryType { get; set; }
        public string PayslipType { get; set; }
        public string Quarter { get; set; }
        public string FestivalBonus { get; set; }
    }
}
