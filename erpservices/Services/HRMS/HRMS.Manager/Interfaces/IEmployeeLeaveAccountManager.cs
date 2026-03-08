using Core;
using HRMS.Manager.Dto;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Manager
{
    public interface IEmployeeLeaveAccountManager
    {        
        Task<List<EmployeeLeaveAccountDto>> GetEmployeeLeaveAccountTables();
        Task<List<EmployeeLeaveAccountDto>> GetEmployeeLeaveAccountListWithDetails();
        Task<List<EmployeeLeaveAccountDto>> GetGenerateChildList(EmployeeLeaveAccountDto elAccount);
        Task<List<EmployeeLeaveAccountDto>> GetEmployeeLeaveAccount(int finYearID, int empoid);
        Task<EmployeeLeaveAccountDto> SaveChanges(EmployeeLeaveAccountDto elaccount, List<EmployeeLeaveAccountDto> childs);
        Task RemoveEmployeeLeaveAccount(int FinancialYearID, int EmployeeID);
        Task AssignLeaveToAllEmployee(int FinancialYearID);
        
        Task<bool> GetExistingPolicy(EmployeeLeaveAccountDto elAccount);
        Task<bool> GetExistingAccountByEmployee(EmployeeLeaveAccountDto elAccount);



    }
}
