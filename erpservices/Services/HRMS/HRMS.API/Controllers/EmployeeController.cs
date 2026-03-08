using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.API.Models;
using System.Threading.Tasks;
using System.Security.Cryptography.Xml;
using System.Collections.Generic;
using Security.DAL.Entities;
using System.IO;
using System;
using Core.Extensions;
using Core.AppContexts;
using Core;
using HRMS.Manager.Interfaces;
using HRMS.Manager.Dto;
using DAL.Core;
using Manager.Core;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Linq;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class EmployeeController : BaseController
    {
        private readonly IEmployeeManager Manager;

        public EmployeeController(IEmployeeManager manager)
        {
            Manager = manager;
        }



        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetEmployeeListDicAsync(parameters);
            return OkResult(new { parentDataSource = model });
        }

        // GET: /Employee/GetAll
        [HttpPost("GetAllEmployeeDirectory")]
        public async Task<IActionResult> GetAllEmployeeDirectory([FromBody] GridParameter parameters)
        {
            var model = await Manager.GetEmployeeDirectoryList(parameters);
            return OkResult(new { parentDataSource = model });
        }

        // GET: /Employee/Get/{primaryID}
        [HttpGet("Get/{EmployeeID:int}")]
        public async Task<IActionResult> Get(int EmployeeID)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var employee = await Manager.GetEmployeeTableDic(EmployeeID);
            var employment = await Manager.GetEmploymentTableDic(EmployeeID);
            var employeeSupervisorMap = await Manager.GetEmployeeSupervisorMap(EmployeeID);
            var dottedSupervisor = await Manager.GetDottedEmployeeSupervisorMap(EmployeeID);
            var delegeedSupervisor = await Manager.GetDelegatedEmployeeSupervisor(EmployeeID);
            return OkResult(new { EmployeeModel = employee, EmploymentModel = employment, EmployeeSupervisorMapModel = employeeSupervisorMap, DottedSupervisor = dottedSupervisor, DelegatedSupervisor = delegeedSupervisor });
        }
        [HttpGet("GetEmployeeByID/{EmployeeID:int}")]
        public async Task<IActionResult> GetEmployeeByID(int EmployeeID)
        {
            var employee = await Manager.GetEmployeeByID(EmployeeID);
            return OkResult(new { EmployeeModel = employee });
        }

        [HttpGet("GetDuplicateEmployeeCode")]
        public async Task<ActionResult> GetDuplicateEmployeeCode(string EmployeeCode)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");

            bool isExists = await Manager.GetDuplicateEmployeeCode(EmployeeCode);

            return OkResult(isExists);
        }
        [HttpGet("SavePersonAsEmployee/{PersonID:int}")]
        public async Task<IActionResult> SavePersonAsEmployee(int PersonID)
        {
            EmployeeSaveModel employee = new EmployeeSaveModel();

            employee.EmployeeModel = await Manager.SavePersonAsEmployee(PersonID);

            return await Get(employee.EmployeeModel.EmployeeID);
        }
        // POST: /Employee/CreateEmployee
        [HttpPost("SaveEmployee")]
        public async Task<IActionResult> SaveEmployee([FromBody] EmployeeSaveModel employeeModel)
        {
            //employeeModel.EmployeeModel = await Manager.SaveChanges(employeeModel.EmployeeModel, employeeModel.EmploymentModel, employeeModel.BankInfoModel, employeeModel.EmployeeSupervisorMapModel);

            //return await Get(employeeModel.EmployeeModel.EmployeeID);
            try
            {
                employeeModel.EmployeeModel = await Manager.SaveChanges(employeeModel.EmployeeModel, employeeModel.EmploymentModel, employeeModel.BankInfoModel, employeeModel.EmployeeSupervisorMapModel);

                return await Get(employeeModel.EmployeeModel.EmployeeID);
            }
            catch (Exception ex)
            {

                return OkResult(new { success = false, ex.Message });
            }

        }

        // GET: /Employee/RemoveSecurityGroup
        [HttpGet("RemoveEmployee/{EmployeeID:int}")]
        public async Task<IActionResult> RemoveEmployee(int EmployeeID)
        {
            //await Manager.UpdateAsync(user);
            await Manager.RemoveEmployee(EmployeeID);
            return OkResult(new { success = true });
        }

        // GET: /Employee/RemoveSecurityGroup
        [HttpGet("GetMediaList/{EmployeeID:int}")]
        public async Task<IActionResult> GetMediaList(int EmployeeID)
        {
            //await Manager.UpdateAsync(user);
            var medialist = await Manager.GetMediaList(EmployeeID);
            return OkResult(medialist);
        }


        [HttpGet("GetAllEmployeeListByWhereCondition")]
        public async Task<ActionResult> GetAllEmployeeListByWhereCondition(string WhereCondition)
        {
            var employeeList = Manager.GetAllEmployeeListByWhereCondition(WhereCondition);

            return OkResult(employeeList.Result);
        }

        // GET: /Employee/Get/{primaryID}
        [HttpGet("GetEmployeeSupervisors/{EmployeeID:int}")]
        public async Task<IActionResult> GetEmployeeSupervisors(int EmployeeID)
        {
            EmployeeID = EmployeeID.IsZero() ? (int)AppContexts.User.EmployeeID : EmployeeID;
            var employeeSupervisorMap = await Manager.GetEmployeeSupervisorMap(EmployeeID);
            var dottedSupervisor = await Manager.GetDottedEmployeeSupervisorMap(EmployeeID);
            var delegeedSupervisor = await Manager.GetDelegatedEmployeeSupervisor(EmployeeID);
            return OkResult(new { EmployeeSupervisorMapModel = employeeSupervisorMap, DottedSupervisor = dottedSupervisor, DelegatedSupervisor = delegeedSupervisor });
        }
        // GET: /Employee/Get/{EmployeeID}
        [HttpGet("GetEmployeeSupervisorsForEmpDirectory/{EmployeeID:int}")]
        public async Task<IActionResult> GetEmployeeSupervisorsForEmpDirectory(int EmployeeID)
        {
            var employeeSupervisorMap = await Manager.GetEmployeeSupervisorMap(EmployeeID);
            var dottedSupervisor = await Manager.GetDottedEmployeeSupervisorMap(EmployeeID);
            var delegeedSupervisor = await Manager.GetDelegatedEmployeeSupervisor(EmployeeID);
            return OkResult(new { EmployeeSupervisorMapModel = employeeSupervisorMap, DottedSupervisor = dottedSupervisor, DelegatedSupervisor = delegeedSupervisor });
        }
        [HttpGet("GetDecryptedJobGrade")]
        public async Task<IActionResult> GetDecryptedJobGrade()
        {
            var jobGrades = await Manager.GetDecryptedJobGrades();
            return OkResult(jobGrades);
        }

        [HttpGet("GetCacheStatus")]
        public async Task<IActionResult> GetCacheStatus()
        {
            var cacheStatus = await Manager.GetCacheStatus();
            return OkResult(cacheStatus);
        }

        [HttpPost("ClearAllCaches")]
        public async Task<IActionResult> ClearAllCaches()
        {
            var result = await Manager.ClearAllCaches();
            return OkResult(new { success = result, message = result ? "All caches cleared successfully" : "Failed to clear caches" });
        }

        [HttpPost("UpdateEmployeeInformation")]
        public async Task<IActionResult> UpdateEmployeeInformation([FromBody] List<EmployeeUpdateInformationDTOForExcel> employees)
        {
        //    if (employees == null || employees.Count == 0)
        //    {
        //        return BadRequest("No employee data provided.");
        //    }
        //    else EmployeeUpdateInformationDTO
        //{

        //        var result = await Manager.UpdateEmployeeInfo(employees);
        //        //var result = await Manager.UpdateEmployeeInfo(employees);

        //        return OkResult(result);
        //    }
         var result = await Manager.UpdateEmployeeInfo(employees);

         return Ok(result);

        }
    }
}

