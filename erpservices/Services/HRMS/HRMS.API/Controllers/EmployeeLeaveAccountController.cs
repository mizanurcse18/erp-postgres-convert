
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using API.Core;
using HRMS.API.Models;
using HRMS.Manager;
using HRMS.Manager.Dto;
using System.Collections.Generic;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class EmployeeLeaveAccountController : BaseController
    {
        private readonly IEmployeeLeaveAccountManager Manager;

        public EmployeeLeaveAccountController(IEmployeeLeaveAccountManager manager)
        {
            Manager = manager;
        }

        // GET: /EmployeeLeaveAccount/GetAll
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var EmployeeLeaveAccounts = await Manager.GetEmployeeLeaveAccountListWithDetails();
            return OkResult(EmployeeLeaveAccounts);

            //return OkResult(new { MasterModel = EmployeeLeaveAccounts });
        }


        // POST: /EmployeeLeaveAccount/CreateEmployeeLeaveAccount
        [HttpPost("SaveEmployeeLeaveAccount")]
        public async Task<IActionResult> SaveEmployeeLeaveAccount([FromBody] EmployeeLeaveAccountSaveModel EmployeeLeaveAccount)
        {
            await Manager.SaveChanges(EmployeeLeaveAccount.MasterModel, EmployeeLeaveAccount.ChildModels);
            
            return await GetLeavePolicy(EmployeeLeaveAccount.MasterModel.FinancialYearID, EmployeeLeaveAccount.MasterModel.EmployeeID);
        }

        // PUT: /EmployeeLeaveAccount/UpdateEmployeeLeaveAccount
        [HttpPut("UpdateEmployeeLeaveAccount")]
        public async Task<IActionResult> UpdateEmployeeLeaveAccount([FromBody] EmployeeLeaveAccountSaveModel EmployeeLeaveAccountUpdate)
        {
            await Manager.SaveChanges(EmployeeLeaveAccountUpdate.MasterModel, EmployeeLeaveAccountUpdate.ChildModels);
            
            return await GetLeavePolicy(EmployeeLeaveAccountUpdate.MasterModel.FinancialYearID , EmployeeLeaveAccountUpdate.MasterModel.EmployeeID);
        }

        // Delete: /EmployeeLeaveAccount/DeleteEmployeeLeaveAccount
        [HttpGet("RemoveEmployeeLeaveAccount")]
        public async Task<IActionResult> RemoveEmployeeLeaveAccount(int FinancialYearID, int EmployeeID)
        {
            await Manager.RemoveEmployeeLeaveAccount(FinancialYearID, EmployeeID);
            return OkResult(new { Success = true });
        }
        [HttpGet("AssignLeaveToAllEmployee")]
        public async Task<IActionResult> AssignLeaveToAllEmployee(int FinancialYearID)
        {
            await Manager.AssignLeaveToAllEmployee(FinancialYearID);
            return OkResult(new { Success = true });
        }
        

        [HttpPost("GetLeaveCategoryWiseListEmp")]
        public async Task<IActionResult> GetLeaveCategoryWiseListEmp([FromBody] EmployeeLeaveAccountDto elAccount)
        {

            elAccount.IsExistsPolicy = await Manager.GetExistingPolicy(elAccount);
            if (!elAccount.IsExistsPolicy && elAccount.FinancialYearID > 0 && elAccount.EmployeeID > 0)
            {
                elAccount.IsExistsPolicy = false;
                elAccount.ErrorMessage = "Leave Policy is not created by this Financial Year and Employee Status.";
            }


            elAccount.IsExists = await Manager.GetExistingAccountByEmployee(elAccount);
            //if(elAccount.IsExists)
            //{
            //    elAccount.IsExists = true;
            //}


            var accountList = new List<EmployeeLeaveAccountDto>();
            if (elAccount.FinancialYearID > 0 && elAccount.EmployeeID > 0)
            {
                accountList = await Manager.GetGenerateChildList(elAccount);
            }
            var clPolicyObj = new
            {
                FinancialYearID = elAccount.FinancialYearID,
                EmployeeID = elAccount.EmployeeID,
                Year = elAccount.IsExists ? accountList[0].Year : elAccount.Year,
                EmployeeName = elAccount.IsExists ? accountList[0].EmployeeName : elAccount.EmployeeName,
                EmployeeStatusName = elAccount.IsExists ? accountList[0].EmployeeStatusName : "",
                DateOfJoining = elAccount.IsExists ? accountList[0].DateOfJoining : null,
                ConfirmDate = elAccount.IsExists ? accountList[0].ConfirmDate : null,
                IsExists = elAccount.IsExists ? true : false,
                IsExistsPolicy = elAccount.IsExistsPolicy ? true : false,
                ErrorMessage = elAccount.ErrorMessage
            };
            return OkResult(new { MasterModel = clPolicyObj, ChildModels = accountList });
        }


        [HttpGet("GetEmployeeLeaveAccount")]
        public async Task<IActionResult> GetLeavePolicy(int FinancialYearID, int EmployeeID)
        {
            var elAccounts = await Manager.GetEmployeeLeaveAccount(FinancialYearID, EmployeeID);

            var elAccountObj = new
            {
                FinancialYearID = FinancialYearID,
                EmployeeID = EmployeeID,
                Year = elAccounts.Count > 0 ? elAccounts[0].Year : 0,
                EmployeeName = elAccounts.Count > 0 ? elAccounts[0].EmployeeName : "",
                EmployeeStatusName = elAccounts.Count > 0 ? elAccounts[0].EmployeeStatusName : "",
                DateOfJoining = elAccounts.Count > 0 ? elAccounts[0].DateOfJoining : null,
                ConfirmDate = elAccounts.Count > 0 ? elAccounts[0].ConfirmDate : null,
                IsExists = elAccounts.Count > 0 ? true : false
            };
            return OkResult(new { MasterModel = elAccountObj, ChildModels = elAccounts });
        }
    }
}
