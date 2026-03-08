using Core;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using Microsoft.AspNetCore.Mvc;
using Security.DAL.Entities;
using Security.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using Core.Extensions;

namespace HRMS.Manager.Interfaces
{
    public interface IEmployeeManager
    {
        Task<List<Employee>> GetEmployeeList();
        GridModel GetEmployeeListDic(GridParameter parameters);
        Task<GridModel> GetEmployeeListDicAsync(GridParameter parameters);
        Task<GridModel> GetEmployeeDirectoryList(GridParameter parameters);        //Task<IEnumerable<Dictionary<string, object>>> GetEmployeeListDic();
        GridModel GetEmployeeDirectoryListSync(GridParameter parameters);
        Task<Employee> GetEmployeeTable(int primaryID);
        public Task<Dictionary<string, object>> GetEmployeeTableDic(int primaryID);
        public Task<Dictionary<string, object>> GetEmployeeByID(int primaryID); 
        public Task<Dictionary<string, object>> GetEmploymentTableDic(int primaryID);
        public Task<List<Dictionary<string, object>>> GetEmployeeSupervisorMap(int primaryID);
        public Task<List<Dictionary<string, object>>> GetDottedEmployeeSupervisorMap(int primaryID);
        public Task<Dictionary<string, object>> GetDelegatedEmployeeSupervisor(int primaryID);
        Task<Employee> SaveChanges(Employee master, Employment employment,EmployeeBankInfo bankInfo, List<EmployeeSupervisorMap> employeeSupervisorMap);
        Task<Employee> SavePersonAsEmployee(int PersonID);
        Task<Employee> SaveChanges(Employee master);
        Task Delete(string EmployeeID);
        Task RemoveEmployee(int EmployeeID);
        Task<string> GetMediaList(int personID);
        Task<bool> GetDuplicateEmployeeCode(string EmployeeCode);
        Task<List<Dictionary<string, object>>> GetAllEmployeeListByWhereCondition(string whereCondition);
        Task<List<JobGradeDecryptDto>> GetDecryptedJobGrades();
        Task<Dictionary<string, object>> GetCacheStatus();
        Task<bool> ClearAllCaches();
        Task<GenericResponse<EmployeeUpdateInformationDTO>> UpdateEmployeeInfo(List<EmployeeUpdateInformationDTOForExcel> employees);

    }
}
