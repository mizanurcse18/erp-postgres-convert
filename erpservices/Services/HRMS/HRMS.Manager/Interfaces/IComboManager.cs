using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Interfaces
{
    public interface IComboManager
    {

        Task<List<ComboModel>> GetDepartments();
        Task<List<ComboModel>> GetAllRenovationORMaintenanceCategory();
         Task<List<ComboModel>> GetDepartmentsCascade(int DivisionID);
         Task<List<ComboModel>> GetDepartmentsCascadeByDivisionIDs(string DivisionIDs);
        Task<List<ComboModel>> GetDesignations();
        Task<List<ComboModel>> GetDivisions();
        Task<List<ComboModel>> GetLeaveTypes();
         Task<List<ComboModel>> GetClusters();
        Task<List<ComboModel>> GetBranchinfos(int RegionID);
        Task<List<ComboModel>> GetRegions(int ClusterID);
        Task<IEnumerable<Dictionary<string, object>>> GetEmployeeDetailsInfo();
        Task<List<ComboModel>> GetEmployeePersons();
        Task<List<ComboModel>> GetRegionsForBranch();
        Task<List<ComboModel>> GetDaysOfWeeks();
        Task<List<ComboModel>> GetEmployees();
        Task<List<ComboModel>> GetDivisionHead();
        Task<List<ComboModel>> GetEmployeesOnlySCM(int DivisionID); 
         Task<List<ComboModel>> GetBackupEmployees();
        Task<List<ComboModel>> GetEmployeesByDepartment(int DepartmentID);
        Task<List<ComboModel>> GetActiveEmployeesByDepartment(int DepartmentID);
        Task<List<ComboModel>> GetActiveEmployeeList();
        Task<List<ComboModel>> GetActiveBackUpEmployeeList();
        Task<List<ComboModel>> GetActiveBackUpEmployeeListForHr(int EmployeeID);
        Task<List<ComboModel>> GetActiveEmployeeListByDeptAndStatus(string deptIDs, string typeIDs, DateTime CutOffDate, int finid);
        Task<List<ComboModel>> GetJobGradeList();
        Task<List<ComboModel>> GetPendingEmployees();
        Task<List<ComboModel>> GetEmployeesWithDivDept();
        Task<IEnumerable<Dictionary<string, object>>> GetEmployeeComboList();
        Task<IEnumerable<Dictionary<string, object>>> GetExitInterviewEmployeesWithDivDept();
        Task<IEnumerable<Dictionary<string, object>>> GetAllEmployeesForAccessDeactivation();
        //Task<IEnumerable<Dictionary<string, object>>> GetAllSupportRequestType();
        Task<IEnumerable<Dictionary<string, object>>> GetFinancialAndAssessmentYear();
        Task<List<ComboModel>> GetEmployeesReligion(int EmployeeID);
        Task<List<ComboModel>> GetDivisionsForWallet(int CWID);
        Task<List<ComboModel>> GetAuditQuestions();
        Task<List<ComboModel>> GetUserWiseUddoktaMerchant();
        Task<List<ComboModel>> GetEmployeesFilteringCascading(int ActiveInActiveID, int DivisionID, int DepartmentID);
        Task<List<ComboModel>> GetEmployeeBirthDate();


    }
}
