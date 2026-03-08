using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class PaySlipDataModel
    {
        public List<EmployeePaySlipDto> paySlipDtos { get; set; }
        public List<EmployeePaySlipInfo> employeePaySlipInfos { get; set; }
        public bool IsValid { get; set; } = true;
        public string message { get; set; }
    }

    public class RegularIncentiveDataModel
    {
        public List<RegularIncentiveDto> regularIncentiveDtos { get; set; }
        public List<EmployeeRegularIncentiveInfo> employeeRegularIncentiveInfos { get; set; }
        public bool IsValid { get; set; } = true;
        public string message { get; set; }
    }

    public class MonthlyIncentiveDataModel
    {
        public List<EmployeeMonthlyIncentiveDto> monthlyIncentiveDtos { get; set; }
        public List<EmployeeMonthlyIncentiveInfo> monthlyIncentiveInfos { get; set; }
        public bool IsValid { get; set; } = true;
        public string message { get; set; }
    }
    public class FestivalBonusDataModel
    {
        public List<EmployeeFestivalBonusDto> festivalBonusDtos { get; set; }
        public List<EmployeeFestivalBonusInfo> festivalBonusInfos { get; set; }
        public bool IsValid { get; set; } = true;
        public string message { get; set; }
    }
}
