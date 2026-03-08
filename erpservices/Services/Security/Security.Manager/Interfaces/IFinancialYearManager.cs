using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager
{
    public interface IFinancialYearManager
    {        
        Task<List<FinancialYearDto>> GetFinancialYearTables();
        Task<IEnumerable<Dictionary<string, object>>> GetFinancialYearListWithDetails();
        Task<FinancialYearDto> GetFinancialYear(int primaryID);
        Task<FinancialYearDto> GetFinancialYearByYear(int year);
        Task<List<PeriodDto>> GetPeriods(int financialYearID);
        Task<FinancialYearDto> SaveChanges(FinancialYearDto financialYear, List<PeriodDto> periods);
        Task RemoveFinancialYear(int FinancialYearID);
        Task<List<PeriodDto>> GetPeriodByID(int financialYearID);
        Task<List<PeriodDto>> GetGenerateChildList(FinancialYearDto finYear);
        
        Task<bool> GetExistingFinancialYear(int year);
    }
}
