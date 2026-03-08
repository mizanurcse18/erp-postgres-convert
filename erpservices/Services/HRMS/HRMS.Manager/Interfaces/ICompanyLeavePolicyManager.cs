using Core;
using HRMS.Manager.Dto;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Manager
{
    public interface ICompanyLeavePolicyManager
    {        
        Task<List<CompanyLeavePolicyDto>> GetCompanyLeavePolicyTables();
        Task<List<CompanyLeavePolicyDto>> GetCompanyLeavePolicyListWithDetails();
        Task<List<CompanyLeavePolicyDto>> GetGenerateChildList(CompanyLeavePolicyDto clPolicy);
        Task<List<CompanyLeavePolicyDto>> GetCompanyLeavePolicy(int finYearID, int empStatusID);
        Task<CompanyLeavePolicyDto> SaveChanges(CompanyLeavePolicyDto financialYear, List<CompanyLeavePolicyDto> periods);
        Task RemoveCompanyLeavePolicy(int FinancialYearID, int EmployeeStatusID);

        Task<bool> GetExistingPolicy(CompanyLeavePolicyDto clPolicy);

    }
}
