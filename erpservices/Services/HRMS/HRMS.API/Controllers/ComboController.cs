using API.Core;
using Core.AppContexts;
using HRMS.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ComboController : BaseController
    {
        private readonly IComboManager Manager;

        public ComboController(IComboManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetSecurityGroupComboSource")]
        public async Task<ActionResult> GetSecurityGroupComboSource()
        {
            //return OkResult(Manager.GetDemoMasterIndependentComboSource());
            var list ="test"+AppContexts.User.UserID;
            return OkResult(list);
        }


        [HttpGet("GetDepartmentsCascade/{DivisionID:int}")]
        public async Task<ActionResult> GetDepartmentsCascade(int DivisionID)
        {
            var list = await Manager.GetDepartmentsCascade(DivisionID);
            return OkResult(list);
        }
        [HttpGet("GetDepartmentsCascadeByDivisionIDs/{DivisionIDs}")]
        public async Task<ActionResult> GetDepartmentsCascadeByDivisionIDs(string DivisionIDs)
        {
            var list = await Manager.GetDepartmentsCascadeByDivisionIDs(DivisionIDs);
            return OkResult(list);
        }
        

        [HttpGet("GetDepartments")]
        public async Task<ActionResult> GetDepartments()
        {
            var list = await Manager.GetDepartments();
            return OkResult(list);
        }

        [HttpGet("GetAuditQuestions")]
        public async Task<ActionResult> GetAuditQuestions()
        {
            var list = await Manager.GetAuditQuestions();
            return OkResult(list);
        }

        [HttpGet("GetAllRenovationORMaintenanceCategory")]
        public async Task<ActionResult> GetAllRenovationORMaintenanceCategory()
        {
            var list = await Manager.GetAllRenovationORMaintenanceCategory();
            return OkResult(list);
        }

        [HttpGet("GetDesignations")]
        public async Task<ActionResult> GetDesignations()
        {
            var list = await Manager.GetDesignations();
            return OkResult(list);
        }
        [HttpGet("GetDivisions")]
        public async Task<ActionResult> GetDivisions()
        {
            var list = await Manager.GetDivisions();
            return OkResult(list);
        }
        [HttpGet("GetLeaveTypes")]
        public async Task<ActionResult> GetLeaveTypes()
        {
            var list = await Manager.GetLeaveTypes();
            return OkResult(list);
        }
        [HttpGet("GetClusters")]
        public async Task<ActionResult> GetClusters()
        {
            var list = await Manager.GetClusters();
            return OkResult(list);
        }
        [HttpGet("GetWorkstations/{RegionID:int}")]
        public async Task<ActionResult> GetWorkstations(int RegionID)
        {
            var list = await Manager.GetBranchinfos(RegionID);
            return OkResult(list);
        }
        //[HttpGet("GetEmployeestatus")]
        //public async Task<ActionResult> GetEmployeestatus()
        //{
        //    //var list = await Manager.GetBranchinfos();
        //    return OkResult(list);
        //}
        [HttpGet("GetRegions/{ClusterID:int}")]
        public async Task<ActionResult> GetRegions(int ClusterID)
        {
            var list = await Manager.GetRegions(ClusterID);
            return OkResult(list);
        }
        [HttpGet("GetRegionsForBranch")]
        public async Task<ActionResult> GetRegionsForBranch()
        {
            var list = await Manager.GetRegionsForBranch();
            return OkResult(list);
        }
        [HttpGet("GetEmployeePersons")]
        public async Task<ActionResult> GetEmployeePersons()
        {
            var list = await Manager.GetEmployeePersons();
            return OkResult(list);
        }
        [HttpGet("GetDaysOfWeeks")]
        public async Task<ActionResult> GetDaysOfWeeks()
        {
            var list = await Manager.GetDaysOfWeeks();
            return OkResult(list);
        }
        [HttpGet("GetEmployees")]
        public async Task<ActionResult> GetEmployees()
        {
            var list = await Manager.GetEmployees();
            return OkResult(list);
        }

        //Get Department Head
        [HttpGet("GetDivisionHead")]
        public async Task<ActionResult> GetDivisionHead()
        {
            var list = await Manager.GetDivisionHead();
            return OkResult(list);
        }

        [HttpGet("GetJobGrade")]
        public async Task<ActionResult> GetJobGrade()
        {
            var list = await Manager.GetJobGradeList();
            return OkResult(list);
        }
        
        [HttpGet("GetEmployeesOnlySCM/{DivisionID:int}")]
        public async Task<ActionResult> GetEmployeesOnlySCM(int DivisionID)
        {
            var list = await Manager.GetEmployeesOnlySCM(DivisionID);
            return OkResult(list);
        }
        
       [HttpGet("GetBackupEmployees")]
        public async Task<ActionResult> GetBackupEmployees()
        {
            var list = await Manager.GetBackupEmployees();
            return OkResult(list);
        }

        [HttpGet("GetEmployeesByDepartment/{DepartmentID:int}")]
        public async Task<ActionResult> GetEmployeesByDepartment(int DepartmentID)
        {
            var list = await Manager.GetEmployeesByDepartment(DepartmentID);
            return OkResult(list);
        }

        [HttpGet("GetActiveEmployeesByDepartment/{DepartmentID:int}")]
        public async Task<ActionResult> GetActiveEmployeesByDepartment(int DepartmentID)
        {
            var list = await Manager.GetActiveEmployeesByDepartment(DepartmentID);
            return OkResult(list);
        }

        // GET: /Employee/Get/{primaryID}
        [HttpGet("GetActiveEmployeeList")]
        public async Task<IActionResult> GetActiveEmployeeList()
        {
            var employeeList = await Manager.GetActiveEmployeeList();
            return OkResult(employeeList);
        }

        [HttpGet("GetActiveBackUpEmployeeList")]
        public async Task<IActionResult> GetActiveBackUpEmployeeList()
        {
            var backupemployeeList = await Manager.GetActiveBackUpEmployeeList();
            return OkResult(backupemployeeList);
        }
        [HttpGet("GetActiveBackUpEmployeeListForHr/{EmployeeID}")]
        public async Task<IActionResult> GetActiveBackUpEmployeeListForHr(int EmployeeID)
        {
            var backupemployeeList = await Manager.GetActiveBackUpEmployeeListForHr(EmployeeID);
            return OkResult(backupemployeeList);
        }


        [HttpGet("GetActiveEmployeeListByDeptAndStatus/{DepartmentIDs}/{EmployeeTypeIDs}/{CutOffDate}/{FinancialYearID}")]
        public async Task<IActionResult> GetActiveEmployeeListByDeptAndStatus(string DepartmentIDs, string EmployeeTypeIDs, DateTime CutOffDate, int FinancialYearID)
        {
            var employeeList = await Manager.GetActiveEmployeeListByDeptAndStatus(DepartmentIDs, EmployeeTypeIDs, CutOffDate, FinancialYearID);
            return OkResult(employeeList);
        }
        [HttpGet("GetJobGrades")]
        public async Task<ActionResult> GetJobGrades()
        {
            var list = await Manager.GetJobGradeList();
            return OkResult(list);
        }
        [HttpGet("GetPendingEmployees")]
        public async Task<ActionResult> GetPendingEmployees()
        {
            var list = await Manager.GetPendingEmployees();
            return OkResult(list);
        }
        [HttpGet("GetEmployeesWithDivDept")]
        public async Task<ActionResult> GetEmployeesWithDivDept()
        {
            var list = await Manager.GetEmployeesWithDivDept();
            return OkResult(list);
        }

        [HttpGet("GetEmployeeComboList")]
        public async Task<ActionResult> GetEmployeeComboList(string param)
        {
            var list = await Manager.GetEmployeeComboList();
            return OkResult(list);
        }

        [HttpGet("GetEmployeesExitInterview")]
        public async Task<ActionResult> GetEmployeesExitInterview()
        {
            var list = await Manager.GetExitInterviewEmployeesWithDivDept();
            return OkResult(list);
        }

        [HttpGet("GetAllEmployeesForAccessDeactivation")]
        public async Task<ActionResult> GetAllEmployeesForAccessDeactivation()
        {
            var list = await Manager.GetAllEmployeesForAccessDeactivation();
            return OkResult(list);
        }
        //[HttpGet("GetAllSupportRequestType")]
        //public async Task<ActionResult> GetAllSupportRequestType()
        //{
        //    var list = await Manager.GetAllEmployeesForAccessDeactivation();
        //    return OkResult(list);
        //}
        //GetEmployeeDetailsInfo
        [HttpGet("GetEmployeeDetailsInfo")]
        public async Task<ActionResult> GetEmployeeDetailsInfo()
        {
            var list = await Manager.GetEmployeeDetailsInfo();
            return OkResult(list);
        }
        [HttpGet("GetFinancialAndAssessmentYear")]
        public async Task<ActionResult> GetFinancialAndAssessmentYear()
        {
            var list = await Manager.GetFinancialAndAssessmentYear();
            return OkResult(list);
        }

        [HttpGet("GetEmployeesReligion/{EmployeeID:int}")]
        public async Task<ActionResult> GetEmployeesReligion(int EmployeeID)
        {
            var list = await Manager.GetEmployeesReligion(EmployeeID);
            return OkResult(list);
        }

        [HttpGet("GetDivisionsForWallet/{CWID:int}")]
        public async Task<ActionResult> GetDivisionsForWallet(int CWID)
        {
            var list = await Manager.GetDivisionsForWallet(CWID);
            return OkResult(list);
        }


        [HttpGet("GetUserWiseUddoktaMerchant")]
        public async Task<ActionResult> GetUserWiseUddoktaMerchant()
        {
            var list = await Manager.GetUserWiseUddoktaMerchant();
            return OkResult(list);
        }

        //
        [HttpGet("GetEmployeesFilteringCascading/{employeeStatusId:int}/{divisionId:int}/{departmentId:int}")]
        public async Task<ActionResult> GetEmployeesFilteringCascading(int employeeStatusId, int divisionId, int departmentId)
        {
            var list = await Manager.GetEmployeesFilteringCascading(employeeStatusId, divisionId, departmentId);
            return OkResult(list);
        }

        [HttpGet("GetEmployeeBirthDate")]
        public async Task<ActionResult> GetEmployeeBirthDate()
        {
            var list = await Manager.GetEmployeeBirthDate();
            return OkResult(list);
        }

    }
}